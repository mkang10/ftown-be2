using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayOSService> _logger;
        public PayOSService(IConfiguration configuration, HttpClient httpClient, ILogger<PayOSService> logger)
        {
            string apiKey = configuration["PayOS:ApiKey"];
            string clientId = configuration["PayOS:ClientId"];
            string checksumKey = configuration["PayOS:ChecksumKey"];
            string webhookUrl = configuration["PayOS:WebhookUrl"];

            _payOS = new PayOS(clientId, apiKey, checksumKey);
            _httpClient = httpClient;
            _logger = logger;
            try
            {
                _payOS.confirmWebhook(webhookUrl);
                _logger.LogInformation($"Webhook URL registered successfully: {webhookUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Webhook URL with payOS");
            }

        }

        public async Task<string?> CreatePayment(int orderId, decimal amount, string paymentMethod)
        {
            var items = new List<ItemData> {
            new ItemData("Đơn hàng #" + orderId, 1, Convert.ToInt32(amount))

        };

            var paymentData = new PaymentData(
                orderCode: orderId,
                amount: (int)amount,
                description: $"Thanh toán đơn hàng {orderId}",
                items: items,
                cancelUrl: "http://localhost:7266/api/payment/cancel",
                returnUrl: $"http://localhost:3000/order-confirmation?orderId={orderId}"
            );

            var createPayment = await _payOS.createPaymentLink(paymentData);
            return createPayment?.checkoutUrl;
        }

        public async Task<PaymentLinkInformation?> GetPaymentStatus(int orderId)
        {
            return await _payOS.getPaymentLinkInformation(orderId);
        }

        public async Task<bool> CancelPayment(int orderId)
        {
            var cancelledPayment = await _payOS.cancelPaymentLink(orderId, "Hủy đơn hàng từ hệ thống");
            return cancelledPayment != null;
        }
    }
}
