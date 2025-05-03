using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Common_Model;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly GetOrderDetailHandler _getOrderDetailHandler;  
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllReturnRequestsHandler> _logger;
        public GetAllReturnRequestsHandler(
            IReturnOrderRepository returnOrderRepository,
            GetOrderDetailHandler getOrderDetailHandler,   
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper

        )
        {
            _returnOrderRepository = returnOrderRepository;
            _getOrderDetailHandler = getOrderDetailHandler;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
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
            // 1️⃣ Lấy trang ReturnOrder đã filter
            var pagedRo = await _returnOrderRepository.GetReturnOrdersAsync(
                status,
                returnOption,
                dateFrom,
                dateTo,
                orderId,
                pageNumber,
                pageSize);

            var dtoItems = new List<ReturnRequestResponse>();

            foreach (var ro in pagedRo.Items)
            {
                // 2️⃣ Lấy thông tin chi tiết đơn hàng
                var orderDetail = await GetOrderDetailForStaffAsync(ro.OrderId);

                // 3️⃣ Parse hình ảnh
                List<string>? images = null;
                if (!string.IsNullOrEmpty(ro.ReturnImages))
                {
                    images = JsonConvert.DeserializeObject<List<string>>(ro.ReturnImages);
                }

                // 4️⃣ Mapping ReturnItems (các sản phẩm muốn đổi/trả)
                var returnItems = _mapper.Map<List<ReturnItemResponse>>(ro.ReturnOrderItems);

                var variantIds = returnItems.Select(i => i.ProductVariantId).Distinct().ToList();
                var variantDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(variantIds);

                // Bổ sung thông tin còn thiếu từ ProductVariant
                foreach (var item in returnItems)
                {
                    if (variantDict.TryGetValue(item.ProductVariantId, out var variant))
                    {
                        item.ProductName = variant.ProductName;
                        item.Color = variant.Color;
                        item.Size = variant.Size;
                        item.ImageUrl = variant.ImagePath;
                        item.Price = variant.Price;
                    }
                }

                // 5️⃣ Map ReturnOrder → ReturnRequestResponse
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
                    Order = orderDetail,
                    ReturnItems = returnItems // ✅ Thêm danh sách sản phẩm đổi/trả
                });
            }

            // 6️⃣ Trả kết quả phân trang
            return new PaginatedResult<ReturnRequestResponse>(
                items: dtoItems,
                totalCount: pagedRo.TotalCount,
                pageNumber: pagedRo.PageNumber,
                pageSize: pagedRo.PageSize
            );
        }

        private async Task<OrderDetailResponseWrapper?> GetOrderDetailForStaffAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            var orderItemsResponses = _mapper.Map<List<OrderItemResponse>>(order.OrderDetails);
            var variantIds = orderItemsResponses.Select(d => d.ProductVariantId).Distinct().ToList();

            Dictionary<int, ProductVariantResponse> variantDetailsDict = new();
            try
            {
                variantDetailsDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(variantIds);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[StaffMode] Error fetching variant details: {ex.Message}");
            }

            foreach (var detail in orderItemsResponses)
            {
                if (variantDetailsDict.TryGetValue(detail.ProductVariantId, out var variantDetails))
                {
                    detail.ProductId = variantDetails.ProductId;
                    detail.ProductName = variantDetails.ProductName;
                    detail.Color = variantDetails.Color;
                    detail.Size = variantDetails.Size;
                    detail.ImageUrl = variantDetails.ImagePath;
                    detail.Price = variantDetails.Price;
                    detail.DiscountApplied = variantDetails.DiscountedPrice;
                }
                else
                {
                    detail.ProductId = 0;
                    detail.ProductName = "Không xác định";
                    detail.Color = "Không xác định";
                    detail.Size = "Không xác định";
                    detail.ImageUrl = "Không xác định";
                    detail.Price = 0;
                    detail.DiscountApplied = 0;
                }
            }

            var paymentMethod = await _paymentRepository.GetPaymentMethodByOrderIdAsync(orderId) ?? "Không xác định";

            return new OrderDetailResponseWrapper
            {
                OrderId = order.OrderId,
                FullName = order.FullName,
                Email = order.Email,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                City = order.City,
                District = order.District,
                Province = order.Province,
                Country = order.Country,
                PaymentMethod = paymentMethod,
                OrderTotal = order.OrderTotal ?? 0,
                ShippingCost = order.ShippingCost ?? 0,
                OrderItems = orderItemsResponses,
                Status = order.Status,
                CreatedDate = order.CreatedDate,
                Ghnid = order.Ghnid
            };
        }

    }

}
