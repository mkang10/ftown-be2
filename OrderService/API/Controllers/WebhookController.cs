using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Net.payOS;
using Microsoft.Extensions.Logging;
using Application.DTO.Request;
using Application.UseCases;

namespace API.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly ILogger<WebhookController> _logger;
        private readonly UpdateOrderStatusHandler _updateOrderStatusHandler;
        private WebhookType webhookBody;

        public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger, UpdateOrderStatusHandler updateOrderStatusHandler)
        {
            _updateOrderStatusHandler = updateOrderStatusHandler;
            string apiKey = configuration["PayOS:ApiKey"];
            string clientId = configuration["PayOS:ClientId"];
            string checksumKey = configuration["PayOS:ChecksumKey"];
            _logger = logger;
            _payOS = new PayOS(clientId, apiKey, checksumKey);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] dynamic payload)
        {
            try
            {
                _logger.LogInformation("Received Webhook from payOS");

                // Xác thực dữ liệu webhook từ payOS
                WebhookData webhookData = _payOS.verifyPaymentWebhookData(webhookBody);

                int orderId = payload["data"]?["orderCode"]?.Value<int>() ?? 0;
                if (orderId == 0)
                {
                    _logger.LogWarning("[Webhook] Missing or invalid orderCode in payload.");
                    return BadRequest(new { message = "Invalid orderCode in webhook data" });
                }
                if (webhookData != null)
                {
                    string paymentStatus = webhookData.code switch
                    {
                        "00" => "Success",
                        "01" => "Pending",
                        "02" => "Failed",
                        _ => "Unknown"
                    };

                    _logger.LogInformation($"Updating Order {webhookData.orderCode} with PaymentStatus: {paymentStatus}");

                    // Tạo DTO và gửi vào Handler
                    var updateRequest = new UpdateOrderStatusRequest
                    {
                        OrderId = webhookData.orderCode,
                        PaymentStatus = paymentStatus
                    };

                    bool updateSuccess = await _updateOrderStatusHandler.Handle(updateRequest);

                    if (updateSuccess)
                    {
                        return Ok(new { message = "Webhook processed successfully" });
                    }

                    return StatusCode(500, new { message = "Failed to update order status" });
                }

                return BadRequest(new { message = "Invalid webhook data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, new { message = "Error processing webhook", error = ex.Message });
            }
        }

    }
}
