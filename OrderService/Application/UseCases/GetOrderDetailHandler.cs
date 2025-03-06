using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetOrderDetailHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderDetailHandler> _logger;

        public GetOrderDetailHandler(
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IInventoryServiceClient inventoryServiceClient,
            IMapper mapper,
            ILogger<GetOrderDetailHandler> logger)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDetailResponseWrapper?> HandleAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            var orderitemsResponses = _mapper.Map<List<OrderItemResponse>>(order.OrderDetails);

            // Lấy danh sách ProductVariantId duy nhất
            var variantIds = orderitemsResponses.Select(d => d.ProductVariantId).Distinct().ToList();

            // Gửi một request duy nhất để lấy thông tin tất cả product variants
            Dictionary<int, ProductVariantResponse> variantDetailsDict = new();
            StoreResponse storeDetails = null;

            try
            {
                variantDetailsDict = await _inventoryServiceClient.GetAllProductVariantsByIdsAsync(variantIds);

                if (order.StoreId.HasValue)
                {
                    storeDetails = await _inventoryServiceClient.GetStoreByIdAsync(order.StoreId.Value);
                    //_logger.LogInformation($"Fetched store details: {JsonSerializer.Serialize(storeDetails)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching product/store details: {ex.Message}");
            }

            // Lấy tồn kho từ StoreStock

            // Ánh xạ thông tin sản phẩm
            foreach (var detail in orderitemsResponses)
            {
                if (variantDetailsDict.TryGetValue(detail.ProductVariantId, out var variantDetails))
                {
                    detail.ProductName = variantDetails.ProductName;
                    detail.Color = variantDetails.Color;
                    detail.Size = variantDetails.Size;
                    detail.ImageUrl = variantDetails.ImagePath;
                   
                }
                else
                {
                    detail.ProductName = "Không xác định";
                    detail.Color = "Không xác định";
                    detail.Size = "Không xác định";
                    detail.ImageUrl = "Không xác định";
                }

                // Lấy số lượng tồn kho từ StoreStock
            }

            // Lấy phương thức thanh toán từ bảng Payment
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
                Store = storeDetails,
                OrderTotal = order.OrderTotal ?? 0,
                ShippingCost = order.ShippingCost ?? 0,
                OrderItems = orderitemsResponses
            };
        }
    }

}
