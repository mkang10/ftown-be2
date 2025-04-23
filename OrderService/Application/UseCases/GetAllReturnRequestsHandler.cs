using Application.DTO.Response;
using Domain.Common_Model;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllReturnRequestsHandler
    {
        private readonly IReturnOrderRepository _returnOrderRepository;
        private readonly GetOrderDetailHandler _getOrderDetailHandler;  // thêm

        public GetAllReturnRequestsHandler(
            IReturnOrderRepository returnOrderRepository,
            GetOrderDetailHandler getOrderDetailHandler    // thêm
        )
        {
            _returnOrderRepository = returnOrderRepository;
            _getOrderDetailHandler = getOrderDetailHandler;
        }

        public async Task<PaginatedResult<ReturnRequestResponse>> HandleAsync(
            string? status,
            string? returnOption,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? orderId,
            int pageNumber,
            int pageSize)
        {
            // 1️⃣ Lấy về trang dữ liệu ReturnOrder, đã filter cả status, returnOption, dateFrom/dateTo, orderId
            var pagedRo = await _returnOrderRepository
                .GetReturnOrdersAsync(
                    status,
                    returnOption,
                    dateFrom,
                    dateTo,
                    orderId,
                    pageNumber,
                    pageSize);

            // 2️⃣ Map ReturnOrder → ReturnRequestResponse
            var dtoItems = new List<ReturnRequestResponse>();
            foreach (var ro in pagedRo.Items)
            {
                var orderDetail = await _getOrderDetailHandler.HandleAsync(ro.OrderId);

                List<string>? images = null;
                if (!string.IsNullOrEmpty(ro.ReturnImages))
                {
                    images = JsonConvert.DeserializeObject<List<string>>(ro.ReturnImages);
                }

                dtoItems.Add(new ReturnRequestResponse
                {
                    ReturnOrderId = ro.ReturnOrderId,
                    OrderId = ro.OrderId,
                    Status = ro.Status,
                    CreatedDate = ro.CreatedDate,
                    TotalRefundAmount = ro.TotalRefundAmount,
                    RefundMethod = ro.RefundMethod,
                    ReturnReason = ro.ReturnReason,
                    ReturnOption = ro.ReturnOption,
                    ReturnDescription = ro.ReturnDescription,
                    ReturnImages = images,
                    BankName = ro.BankName,
                    BankAccountNumber = ro.BankAccountNumber,
                    BankAccountName = ro.BankAccountName,
                    Order = orderDetail
                });
            }

            // 3️⃣ Trả về PaginatedResult với danh sách DTO
            return new PaginatedResult<ReturnRequestResponse>(
                items: dtoItems,
                totalCount: pagedRo.TotalCount,
                pageNumber: pagedRo.PageNumber,
                pageSize: pagedRo.PageSize
            );
        }
    }

}
