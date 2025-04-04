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
        private readonly GHNLogHandler _logHandler;

        public GHNController(HttpClient httpClient, GHNLogHandler logHandler)
        {
            _httpClient = httpClient;
            _logHandler = logHandler;
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
                //=====here===========
                //====================
                return Ok(responseData);
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpPost("order-status-list")]
        public async Task<IActionResult> GetOrderStatusListl([FromBody] OrderDetailRequest orderDetailRequest)
        {
            if (orderDetailRequest == null || string.IsNullOrEmpty(orderDetailRequest.order_code))
            {
                return BadRequest("Invalid order code.");
            }

            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(orderDetailRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            // add token
            content.Headers.Add("Token", token);
            var response = await _httpClient.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();

                // Parse cái Jison ra , giải mã nó
                var jsonResponse = JObject.Parse(responseData);

                // Lấy danh sách log
                var logs = jsonResponse["data"]["log"].ToObject<List<Application.DTO.Response.LogEntry>>();

                // Lấy trạng thái mới nhất theo thời gian
                var latestStatuses = logs
                    .GroupBy(log => log.Status) // Nhóm theo trạng thái
                    .Select(g => g.OrderByDescending(log => log.UpdatedDate).First()) // Lấy log mới nhất trong mỗi nhóm
                    .Select(log => new { log.Status, log.UpdatedDate }) // Chọn trạng thái và ngày cập nhật
                    .ToList();
                if (latestStatuses == null)
                {
                    return BadRequest();

                }

                return Ok(new { latestStatuses });
            }

            var errorResponseData = await response.Content.ReadAsStringAsync();
            return BadRequest(errorResponseData);
        }


        [HttpPost("order-status-newest")]
        public async Task<IActionResult> GetOrderStatusNewest([FromBody] OrderDetailRequest orderDetailRequest)
        {
            if (orderDetailRequest == null || string.IsNullOrEmpty(orderDetailRequest.order_code))
            {
                return BadRequest("Invalid order code.");
            }

            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(orderDetailRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            // add token
            content.Headers.Add("Token", token);
            var response = await _httpClient.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseData);
                var logs = jsonResponse["data"]["log"].ToObject<List<Application.DTO.Response.LogEntry>>();

                // Lấy trạng thái mới nhất
                var latestStatus = logs
                    .OrderByDescending(log => log.UpdatedDate)
                    .Select(log => new 
                    { log.Status, 
                        log.UpdatedDate 
                    })
                    .FirstOrDefault(); // Lấy trạng thái mới nhất

                return Ok(latestStatus);
            }

            var errorResponseData = await response.Content.ReadAsStringAsync();
            return BadRequest(errorResponseData);
        }

        [HttpPost("order-detail")]
        public async Task<IActionResult> GetOrderDetailWithData([FromBody] OrderDetailRequest orderDetailRequest)
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
                // parse JSON ra 
                var jsonResponse = JObject.Parse(responseData);

                // truy vấn từ data xuống mảng log status
                var logs = jsonResponse["data"]["log"].ToObject<List<Application.DTO.Response.LogEntry>>();

                // Lấy trạng thái mới nhất theo thời gian và sắp xếp
                var latestStatuses = logs
                    .GroupBy(log => log.Status)
                    .Select(g => g.OrderByDescending(log => log.UpdatedDate).First())
                    .Select(log => new Application.DTO.Response.LogEntry // gán giá trị vào log entry
                    {
                        Status = log.Status,
                        UpdatedDate = log.UpdatedDate
                    })
                    .ToList();

                // Gán từng giá trị vô để call lên FE cho dễ
                var orderDetail = new OrderDetailDto
                {
                    Items = jsonResponse["data"]["items"].ToObject<List<Application.DTO.Response.Item>>(),
                    RequiredNote = jsonResponse["data"]["required_note"].ToString(),
                    FromName = jsonResponse["data"]["from_name"].ToString(),
                    FromPhone = jsonResponse["data"]["from_phone"].ToString(),
                    FromAddress = jsonResponse["data"]["from_address"].ToString(),
                    ToName = jsonResponse["data"]["to_name"].ToString(),
                    ToPhone = jsonResponse["data"]["to_phone"].ToString(),
                    ToAddress = jsonResponse["data"]["to_address"].ToString(),
                    LatestStatuses = latestStatuses
                };

                return Ok(new { OrderDetail = orderDetail });
            }

            var errorResponseData = await response.Content.ReadAsStringAsync();
            return BadRequest(errorResponseData);
        }
    }
}