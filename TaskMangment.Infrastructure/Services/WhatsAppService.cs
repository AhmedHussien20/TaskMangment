using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;

        public WhatsAppService(HttpClient httpClient, IOptions<WhatsAppSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.Token}");
        }

        public async Task SendTaskAssignedNotification(string phone, string userName, string message)
        {

            var body = new
            {
                to = phone,
                type = "template",
                template = new
                {
                    template_id = "evaluation",
                    language = "ar",
                    argument = new Dictionary<string, object>
                    {
                        ["BODY"] = new[] { userName, message }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("message/send?create=true", body);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Azeer API Error: {response.StatusCode} - {result}");
        }
    }
}

