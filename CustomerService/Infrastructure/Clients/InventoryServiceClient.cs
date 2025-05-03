using Application.DTO.Response;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
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


        public async Task<List<ProductResponse>?> GetAllProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("products/view-all");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi InventoryService: {ex.Message}");
                return null;
            }
        }
        public async Task<ProductDetailResponse?> GetProductByIdAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductDetailResponse>>();

                    if (apiResult != null && apiResult.Status)
                    {
                        return apiResult.Data;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi InventoryService: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ProductResponse>?> GetProductsByStyleNameAsync(string styleName, int page, int pageSize)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/by-style?styleName={Uri.EscapeDataString(styleName)}&page={page}&pageSize={pageSize}");

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<ResponseDTO<List<ProductResponse>>>();

                    if (apiResult != null && apiResult.Status)
                    {
                        return apiResult.Data;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi InventoryService (GetProductsByStyleNameAsync): {ex.Message}");
                return null;
            }
        }
        public async Task<ProductVariantResponse?> GetProductVariantByIdAsync(int variantId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/variant/{variantId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductVariantResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Lỗi khi gọi InventoryService: {ex.Message}");
                return null;
            }
        }
        public async Task<ProductVariantResponse?> GetProductVariantById(int variantId)
        {
            var response = await _httpClient.GetAsync($"products/variant/{variantId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductVariantResponse>>();
            return result?.Data;
        }
        public async Task<ProductVariantResponse?> GetProductVariantByDetails(int productId, string size, string color)
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/variant/details?productId={productId}&size={Uri.EscapeDataString(size)}&color={Uri.EscapeDataString(color)}");

                // ❌ Kiểm tra nếu response không thành công
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"InventoryService trả về mã lỗi {response.StatusCode} khi tìm biến thể sản phẩm.");
                    return null;
                }

                // ✅ Đọc JSON từ response
                var result = await response.Content.ReadFromJsonAsync<ResponseDTO<ProductVariantResponse>>();

                // ❌ Kiểm tra dữ liệu null hoặc API báo lỗi
                if (result == null || !result.Status)
                {
                    _logger.LogWarning($"InventoryService phản hồi lỗi: {result?.Message ?? "Không có dữ liệu"}");
                    return null;
                }

                return result.Data;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Lỗi HTTP khi gọi InventoryService: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi không xác định khi gọi InventoryService: {ex.Message}");
                return null;
            }
        }


    }
}
