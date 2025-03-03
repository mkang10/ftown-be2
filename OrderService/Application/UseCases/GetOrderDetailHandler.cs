using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetOrderDetailHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public GetOrderDetailHandler(IOrderRepository orderRepository, IMapper mapper, IInventoryServiceClient inventoryServiceClient)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<OrderDetailResponseWrapper?> HandleAsync(int orderId)
        {
            // Lấy đơn hàng từ repository, bao gồm OrderDetails (chú ý dùng Include nếu sử dụng EF Core)
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            // Map danh sách OrderDetail sang DTO OrderDetailResponse
            var orderDetailResponses = _mapper.Map<List<OrderDetailResponse>>(order.OrderDetails);

            // Enrich từng OrderDetailResponse với thông tin chi tiết của product variant
            foreach (var detail in orderDetailResponses)
            {
                var variantDetails = await _inventoryServiceClient.GetProductVariantByIdAsync(detail.ProductVariantId);
                if (variantDetails != null)
                {
                    detail.ProductName = variantDetails.ProductName;
                    detail.Color = variantDetails.Color;
                    detail.Size = variantDetails.Size;
                }
            }

            // Tạo wrapper DTO chứa OrderId và danh sách OrderDetailResponse
            var responseWrapper = new OrderDetailResponseWrapper
            {
                OrderId = order.OrderId,
                OrderDetails = orderDetailResponses
            };

            return responseWrapper;
        }
    }
}
