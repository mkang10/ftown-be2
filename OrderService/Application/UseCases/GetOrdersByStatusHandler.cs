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
    public class GetOrdersByStatusHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public GetOrdersByStatusHandler(IOrderRepository orderRepository, IMapper mapper, IInventoryServiceClient inventoryServiceClient)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<List<OrderResponse>> HandleAsync(string status, int? accountId)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status, accountId);
            var orderResponses = _mapper.Map<List<OrderResponse>>(orders);

            // Enrich từng order item với thông tin chi tiết từ ProductVariant thông qua InventoryServiceClient
            foreach (var orderResponse in orderResponses)
            {
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
