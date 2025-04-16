using Domain.Interfaces;
using Infrastructure.HelperServices.Models;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Infrastructure.HelperServices
{
    public class EmailService : IEmailRepository
    {
        private readonly EmailServiceDTO _settings;
        public EmailService(IOptions<EmailServiceDTO> settings)
        {
            _settings = settings.Value;
        }
        public async Task SendInvoiceEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var client = new RestClient(_settings.ApiUrl);
            var request = new RestRequest();
            request.AddHeader("Authorization", $"Bearer {_settings.ApiToken}");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                from = new { email = "hello@example.com", name = "Shop Invoice" },
                to = new[] { new { email = toEmail } },
                subject = subject,
                html = htmlContent, // gửi HTML thay vì text
                category = "Invoice"
            };

            request.AddJsonBody(body);
            var response = await client.ExecutePostAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Email sending failed: {response.Content}");
            }
        }
    }
}
