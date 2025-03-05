using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using Domain.Entities;
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

        public InventoryServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductVariant?> GetProductVariantByIdAsync(int productVariantId)
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
                var result = JsonSerializer.Deserialize<ResponseDTO<ProductVariant>>(responseData, new JsonSerializerOptions
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
                var response = await _httpClient.GetAsync($"api/inventory/stock?storeId={storeId}&variantId={variantId}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy tồn kho: {response.StatusCode}");
                    return 0;
                }

                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Stock API Response: {responseData}");

                // Giả sử API trả về ResponseDTO<int> chứa số lượng tồn kho
                var result = JsonSerializer.Deserialize<ResponseDTO<int>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? 0;
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