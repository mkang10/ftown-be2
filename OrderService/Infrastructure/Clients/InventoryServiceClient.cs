using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Clients
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryServiceClient> _logger;
        public InventoryServiceClient(HttpClient httpClient, ILogger<InventoryServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

        }

        public async Task<ProductVariantResponse?> GetProductVariantByIdAsync(int productVariantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/variant/{productVariantId}");
                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] API Response: {responseData}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy thông tin sản phẩm: {response.StatusCode}");
                    return null;
                }

                // Deserialize thành ResponseDTO<ProductVariant> và trả về Data
                var result = JsonSerializer.Deserialize<ResponseDTO<ProductVariantResponse>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                return result?.Data;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Lỗi kết nối đến InventoryService: {ex.Message}");
                return null;
            }
        }
        public async Task<Dictionary<int, ProductVariantResponse>> GetAllProductVariantsByIdsAsync(List<int> variantIds)
        {
            if (variantIds == null || !variantIds.Any())
            {
                return new Dictionary<int, ProductVariantResponse>();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("products/variants/details", variantIds);
                response.EnsureSuccessStatusCode();

                // 🛠 Đọc dữ liệu đúng kiểu ResponseDTO<List<ProductVariantResponse>>
                var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO<List<ProductVariantResponse>>>();

                // ✅ Lấy danh sách từ responseDTO.Data
                return responseDTO?.Data?.ToDictionary(v => v.VariantId) ?? new Dictionary<int, ProductVariantResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching product variants: {ex.Message}");
                return new Dictionary<int, ProductVariantResponse>();
            }
        }



        public async Task<StoreResponse?> GetStoreByIdAsync(int storeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"stores/{storeId}");
                var responseData = await response.Content.ReadAsStringAsync(); // 🛠 Log toàn bộ JSON response
                Console.WriteLine($"[DEBUG] Store API Response: {responseData}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy thông tin cửa hàng: {response.StatusCode}");
                    return null;
                }

                // ✅ Nếu API trả về ResponseDTO<StoreResponse>, cần đọc từ `.Data`
                var responseDTO = JsonSerializer.Deserialize<ResponseDTO<StoreResponse>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return responseDTO?.Data; // ✅ Lấy dữ liệu từ `Data`
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Lỗi kết nối đến InventoryService: {ex.Message}");
                return null;
            }
        }



        public async Task<List<Store>> GetAllStoresAsync()
        {
            try
            {
                // Gọi endpoint GET api/stores, giả sử API trả về JSON của ResponseDTO<List<Store>>
                var response = await _httpClient.GetAsync("stores");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy danh sách cửa hàng: {response.StatusCode}");
                    return new List<Store>();
                }

                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Stores API Response: {responseData}");

                // Deserialize thành ResponseDTO<List<Store>> và trả về Data
                var result = JsonSerializer.Deserialize<ResponseDTO<List<Store>>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? new List<Store>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Lỗi kết nối đến InventoryService: {ex.Message}");
                return new List<Store>();
            }
        }

        public async Task<int> GetStockQuantityAsync(int storeId, int variantId)
        {
            try
            {
                // Gọi endpoint GET api/inventory/stock?storeId={storeId}&variantId={variantId}
                var response = await _httpClient.GetAsync($"stores/{storeId}/stock/{variantId}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy tồn kho: {response.StatusCode}");
                    return 0;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Stock API Response: {responseData}");

                // Giả sử API trả về ResponseDTO<int> chứa số lượng tồn kho
                var result = JsonSerializer.Deserialize<ResponseDTO<StockQuantityResponse>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                return result?.Data.StockQuantity ?? 0;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Lỗi kết nối đến InventoryService: {ex.Message}");
                return 0;
            }
        }
        /// <summary>
        /// Gửi yêu cầu giảm tồn kho sau khi đơn hàng được tạo thành công.
        /// </summary>
        public async Task<bool> UpdateStockAfterOrderAsync(int storeId, List<OrderDetail> orderDetails)
        {
            try
            {
                // Chuyển đổi danh sách OrderDetail thành StockUpdateRequest
                var stockUpdateRequest = new StockUpdateRequest
                {
                    StoreId = storeId,
                    Items = orderDetails.Select(od => new StockItemResponse
                    {
                        VariantId = od.ProductVariantId,
                        Quantity = od.Quantity
                    }).ToList()
                };
                Console.WriteLine($"[DEBUG] Payload gửi đi: {JsonSerializer.Serialize(stockUpdateRequest)}");
                // Gửi request đến InventoryService
                var response = await _httpClient.PostAsJsonAsync("stores/update-after-order", stockUpdateRequest);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể cập nhật tồn kho: {response.StatusCode}");
                    return false;
                }

                return await response.Content.ReadFromJsonAsync<bool>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi gọi UpdateStockAfterOrderAsync: {ex.Message}");
                return false;
            }
        }
    }
}