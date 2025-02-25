using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

                // Deserialize trực tiếp vào entity ProductVariant
                return JsonSerializer.Deserialize<ProductVariant>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
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
                // Gọi endpoint GET api/stores, giả sử API trả về JSON của List<Store>
                var response = await _httpClient.GetAsync("stores");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy danh sách cửa hàng: {response.StatusCode}");
                    return new List<Store>();
                }

                var responseData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Stores API Response: {responseData}");

                // Deserialize trực tiếp vào List<Store>
                var stores = JsonSerializer.Deserialize<List<Store>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return stores ?? new List<Store>();
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

                // Nếu API trả về số nguyên dạng plain text
                if (int.TryParse(responseData, out int stockQuantity))
                {
                    return stockQuantity;
                }

                // Nếu API trả về JSON thì có thể deserialize như sau:
                // var stockResponse = JsonSerializer.Deserialize<StockResponse>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                // return stockResponse?.StockQuantity ?? 0;

                return 0;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] Lỗi kết nối đến InventoryService: {ex.Message}");
                return 0;
            }
        }
    }
}
