using Application.DTO.Request;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Infrastructure
{
    public class DeepSeekService : IChatServices
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public DeepSeekService(HttpClient httpClient, string baseUrl, string apiKey)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }

        public async Task StreamChatAsync(string userInput, Func<string, Task> onChunkReceived, CancellationToken cancellationToken)
        {
            var requestObj = new ChatCompletionRequest
            {
                Model = "deepseekr1",
                Stream = true,
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "user", Content = userInput }
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var jsonRequest = JsonSerializer.Serialize(requestObj, options);
            using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Nếu API yêu cầu xác thực, bạn có thể thêm header tương ứng, ví dụ:
            // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var url = $"{_baseUrl}/chat/completions";

            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // API trả về theo định dạng "data: {json}"
                if (line.StartsWith("data: "))
                {
                    var jsonData = line.Substring("data: ".Length).Trim();

                    // Nếu nhận được "[DONE]", kết thúc stream
                    if (jsonData == "[DONE]")
                        break;

                    try
                    {
                        var chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(jsonData, options);
                        if (chunk?.Choices != null && chunk.Choices.Count > 0)
                        {
                            var contentDelta = chunk.Choices[0].Delta?.Content;
                            if (!string.IsNullOrEmpty(contentDelta))
                            {
                                await onChunkReceived(contentDelta);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Lỗi khi parse dữ liệu chunk: {ex.Message}");
                    }
                }
            }
        }
    }
}
