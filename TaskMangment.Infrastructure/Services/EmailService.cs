using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly HttpClient _httpClient;

        public EmailService(IOptions<EmailSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(string to, string subject, string body, List<EmailAttachment>? attachments = null)
        {

            
            if (string.IsNullOrWhiteSpace(_settings.BrevoApiKey))
                throw new Exception("BrevoApiKey is missing in EmailSettings.");

            if (string.IsNullOrWhiteSpace(_settings.From))
                throw new Exception("From email is missing in EmailSettings.");

            object[]? brevoAttachments = null;

            if (attachments != null && attachments.Count > 0)
            {
                brevoAttachments = attachments.Select(a => new
                {
                    name = a.Name,
                    content = a.ContentBase64
                }).Cast<object>().ToArray();
            }

            var payload = new
            {
                sender = new
                {
                    email = _settings.From,
                    name = "Task Manager"
                },
                to = new[]
                {
                    new { email = to }
                },
                subject,
                htmlContent = body,
                attachment = brevoAttachments



            };

            string json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", _settings.BrevoApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Brevo email send failed: {(int)response.StatusCode} - {responseBody}");
            }
        }
    }
}
