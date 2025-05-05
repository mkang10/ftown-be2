using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public interface IGoongService
    {
        /// <summary>
        /// Tính khoảng cách (mét) giữa hai cặp tọa độ bằng Goong API.
        /// </summary>
        Task<double> GetDistanceAsync(
            double originLat, double originLng,
            double destLat, double destLng);

        public class GoongService : IGoongService
        {
            private readonly HttpClient _httpClient;
            private readonly string _apiKey;

            public GoongService(HttpClient httpClient, IConfiguration config)
            {
                _httpClient = httpClient;
                _apiKey = config["Goong:ApiKey"];
            }

            public async Task<double> GetDistanceAsync(
                double originLat, double originLng,
                double destLat, double destLng)
            {
                var url = $"https://rsapi.goong.io/Distance" +
                          $"?origins={originLat},{originLng}" +
                          $"&destinations={destLat},{destLng}" +
                          $"&mode=driving" +
                          $"&api_key={_apiKey}";

                var resp = await _httpClient.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);

                // rows[0].elements[0].distance.value (mét)
                return (double)obj["rows"]![0]!["elements"]![0]!["distance"]!["value"];
            }
        }
    }

}
