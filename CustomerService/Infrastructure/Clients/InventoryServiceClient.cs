using Application.DTO.Response;
using Application.Interfaces;
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

        public InventoryServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                return await _httpClient.GetFromJsonAsync<ProductDetailResponse>($"products/{productId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi InventoryService: {ex.Message}");
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
    }
}
