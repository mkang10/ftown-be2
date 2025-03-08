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
    public class GetReturnableOrdersHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public GetReturnableOrdersHandler(IOrderRepository orderRepository, IMapper mapper, IInventoryServiceClient inventoryServiceClient)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<List<OrderResponse>> HandleAsync(int accountId)
        {
            var orders = await _orderRepository.GetReturnableOrdersAsync(accountId);
            var orderResponses = _mapper.Map<List<OrderResponse>>(orders);

            foreach (var orderResponse in orderResponses)
            {
                var order = orders.FirstOrDefault(o => o.OrderId == orderResponse.OrderId);

                if (order != null)
                {
                    // Lấy ngày giao hàng từ OrderHistory (ngày cuối cùng chuyển sang "delivered")
                    var deliveredDate = order.OrderHistories
                        .Where(oh => oh.OrderStatus == "completed")
                        .OrderByDescending(oh => oh.ChangedDate)
                        .Select(oh => oh.ChangedDate)
                        .FirstOrDefault();

                    orderResponse.DeliveredDate = deliveredDate;
                }

                // Lấy thông tin chi tiết sản phẩm từ InventoryServiceClient
                foreach (var item in orderResponse.Items)
                {
                    var variantDetails = await _inventoryServiceClient.GetProductVariantByIdAsync(item.ProductVariantId);
                    if (variantDetails != null)
                    {
                        item.ProductName = variantDetails.ProductName;
                        item.Color = variantDetails.Color;
                        item.Size = variantDetails.Size;
                        item.ImageUrl = variantDetails.ImagePath;
                    }
                }
            }

            return orderResponses;
        }
    }

}
