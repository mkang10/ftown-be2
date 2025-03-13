using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GHNController : ControllerBase
    {
        

        private readonly HttpClient _httpClient;
        private string _url = "https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create";
        private string token = "8c24c3ed-fb9d-11ef-82e7-a688a46b55a3";
        private string shopid = "196109";

        public GHNController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
        {
            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(orderRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            content.Headers.Add("ShopId", shopid);
            content.Headers.Add("Token", token);

            var response = await _httpClient.PostAsync(_url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                // luu thong tin o day
                return Ok(responseData);    
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpPost("order-detail")]
        public async Task<IActionResult> GetOrderDetail([FromBody] OrderDetailRequest orderDetailRequest)
        {
            if (orderDetailRequest == null || string.IsNullOrEmpty(orderDetailRequest.order_code))
            {
                return BadRequest("Invalid order code.");
            }

            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(orderDetailRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            content.Headers.Add("Token", token);

            var response = await _httpClient.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return Ok(responseData);
            }

            var errorResponseData = await response.Content.ReadAsStringAsync();
            return BadRequest(errorResponseData);
        }
    }
}