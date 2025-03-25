using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Azure.Core;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CheckOutHandler
    {
        private readonly IDistributedCache _cache;
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly GetSelectedCartItemsHandler _getSelectedCartItemsHandler;
        private readonly IConfiguration _configuration;
        private readonly ShippingCostHandler _shippingCostHandler;

        public CheckOutHandler(
            IDistributedCache cache,
            ICustomerServiceClient customerServiceClient,
            IShippingAddressRepository shippingAddressRepository,
            IInventoryServiceClient inventoryServiceClient,
            GetSelectedCartItemsHandler getSelectedCartItemsHandler,
            ShippingCostHandler shippingCostHandler,
            IConfiguration configuration)
        {
            _cache = cache;
            _customerServiceClient = customerServiceClient;
            _shippingAddressRepository = shippingAddressRepository;
            _inventoryServiceClient = inventoryServiceClient;
            _getSelectedCartItemsHandler = getSelectedCartItemsHandler;
            _shippingCostHandler = shippingCostHandler;
            _configuration = configuration;
        }

        public async Task<CheckOutResponse?> Handle(CheckOutRequest request)
        {
            var checkOutSessionId = Guid.NewGuid().ToString(); // Tạo Session ID

            // Lấy địa chỉ giao hàng mặc định và tất cả địa chỉ của account 

            var shippingAddresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(request.AccountId);
            if (shippingAddresses == null || !shippingAddresses.Any())
                return null;

            var defaultAddress = await _shippingAddressRepository.GetDefaultShippingAddressAsync(request.AccountId);
            if (defaultAddress == null) return null;

            // Lấy danh sách sản phẩm đã chọn từ giỏ hàng sử dụng GetSelectedCartItemsHandler
            var orderItems = await _getSelectedCartItemsHandler.Handle(request.AccountId, request.SelectedProductVariantIds);
            if (orderItems == null || !orderItems.Any()) return null;

            // Tính tổng tiền sản phẩm
            var totalAmount = orderItems.Sum(item => item.DiscountedPrice * item.Quantity);
            if (totalAmount <= 0) return null;

            // Tính phí vận chuyển dựa vào địa chỉ mặc định
            decimal shippingCost = _shippingCostHandler.CalculateShippingCost(defaultAddress.City, defaultAddress.District);

            // Danh sách phương thức thanh toán
            var availablePaymentMethods = new List<string> { "COD", "PAYOS" };

            // Lưu dữ liệu vào Redis (hết hạn sau 15 phút)
            var checkOutData = new CheckOutData
            {
                AccountId = request.AccountId,
                OrderTotal = totalAmount,
                ShippingCost = shippingCost,
                ShippingAddressId = defaultAddress.AddressId,
                Items = orderItems
            };

            var cacheKey = $"checkout:{checkOutSessionId}";
            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(checkOutData), cacheOptions);

            return new CheckOutResponse
            {
                CheckOutSessionId = checkOutSessionId,
                OrderTotal = totalAmount,
                ShippingCost = shippingCost,
                AvailablePaymentMethods = availablePaymentMethods,
                ShippingAddress = defaultAddress,
                ShippingAddresses = shippingAddresses,
                Items = orderItems.Select(item => new OrderItemResponse
                {
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductName,
                    ImageUrl = item.ImageUrl,
                    Color = item.Color,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    PriceAtPurchase = item.DiscountedPrice,
                    DiscountApplied = item.Price - item.DiscountedPrice // Nếu có logic giảm giá, cập nhật tại đây
                }).ToList()
            };
        }
        
    }
}
