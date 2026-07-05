using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        }

        public async Task SendTaskAssignedNotification(string phone, string userName, string message)
        {
            var url = $"https://api.ultramsg.com/{_settings.InstanceId}/messages/chat";
            var text = string.IsNullOrWhiteSpace(userName) ? message : $"{userName}\n{message}";

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", _settings.Token),
                new KeyValuePair<string, string>("to", NormalizePhone(phone)),
                new KeyValuePair<string, string>("body", text)
            });

            var response = await _httpClient.PostAsync(url, form);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ultramsg API Error: {response.StatusCode} - {result}");
        }

        private static string NormalizePhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.StartsWith("0") && digits.Length > 1 ? digits.TrimStart('0') : digits;
        }
    }
}
