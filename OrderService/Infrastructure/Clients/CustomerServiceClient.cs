using Application.DTO.Response;
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
    public class CustomerServiceClient : ICustomerServiceClient
    {
        private readonly HttpClient _httpClient;

        public CustomerServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CartItem>?> GetCartAsync(int accountId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"cart/{accountId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể lấy giỏ hàng. Mã lỗi: {response.StatusCode}");
                    return null;
                }

                var responseData = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[DEBUG] Dữ liệu từ API: {responseData}"); // ✅ Debug JSON đầu vào

                return JsonSerializer.Deserialize<List<CartItem>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<CartItem>(); // ✅ Trả về danh sách `CartItem`
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi lấy giỏ hàng: {ex.Message}");
                return null;
            }
        }
        public async Task ClearCartAfterOrderAsync(int accountId)
        {
            try
            {
                // Gọi endpoint xóa giỏ hàng sau khi tạo đơn hàng: POST cart/{accountId}/clear-after-order
                var response = await _httpClient.PostAsync($"cart/{accountId}/clear-after-order", null);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ERROR] Không thể xóa giỏ hàng sau khi tạo đơn hàng. Mã lỗi: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi khi xóa giỏ hàng sau khi tạo đơn hàng: {ex.Message}");
            }
        }


    }

}
