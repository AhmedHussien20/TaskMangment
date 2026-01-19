using TaskMangment.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

        public async Task SendEmailAsync(string to, string subject, string body)
        { 

            var payload = new
            {
                sender = new
                {
                    email = "task_for_u@ararhni.com",
                    name = "Task Manager"
                },
                to = new[]
                {
                    new { email = to }
                },
                subject = subject,
                htmlContent = body
            };

            string json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", "");
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
