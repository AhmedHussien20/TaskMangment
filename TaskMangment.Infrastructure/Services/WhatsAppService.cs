using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Notification;
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

        public async Task SendNotificationAsync(
            string phone,
            string userName,
            string message,
            IReadOnlyList<WhatsAppAttachment>? attachments = null)
        {
            EnsureConfigured(phone);

            var text = FormatTextMessage(userName, message);
            await SendChatAsync(phone, text);

            if (attachments == null || attachments.Count == 0)
                return;

            foreach (var attachment in attachments)
            {
                if (string.IsNullOrWhiteSpace(attachment.Url))
                    continue;

                if (IsImage(attachment.ContentType, attachment.FileName))
                    await SendImageAsync(phone, attachment.Url, attachment.FileName);
                else
                    await SendDocumentAsync(phone, attachment.Url, attachment.FileName);
            }
        }

        private void EnsureConfigured(string phone)
        {
            if (string.IsNullOrWhiteSpace(_settings.InstanceId) || string.IsNullOrWhiteSpace(_settings.Token))
                throw new AppException(
                    ErrorCodes.WhatsAppNotConfigured,
                    StatusCodes.Status500InternalServerError,
                    "WhatsApp InstanceId or Token is missing");

            if (string.IsNullOrWhiteSpace(phone))
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status400BadRequest,
                    "Employee mobile number is missing");
        }

        private static string FormatTextMessage(string? userName, string message)
        {
            var body = message?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userName))
                return body;

            return $"{userName.Trim()}\n\n{body}";
        }

        private async Task SendChatAsync(string phone, string text)
        {
            var url = $"https://api.ultramsg.com/{_settings.InstanceId}/messages/chat";
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", _settings.Token),
                new KeyValuePair<string, string>("to", NormalizePhone(phone)),
                new KeyValuePair<string, string>("body", text)
            });

            await PostAsync(url, form);
        }

        private async Task SendImageAsync(string phone, string imageUrl, string fileName)
        {
            var url = $"https://api.ultramsg.com/{_settings.InstanceId}/messages/image";
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", _settings.Token),
                new KeyValuePair<string, string>("to", NormalizePhone(phone)),
                new KeyValuePair<string, string>("image", imageUrl),
                new KeyValuePair<string, string>("caption", fileName)
            });

            await PostAsync(url, form);
        }

        private async Task SendDocumentAsync(string phone, string documentUrl, string fileName)
        {
            var url = $"https://api.ultramsg.com/{_settings.InstanceId}/messages/document";
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", _settings.Token),
                new KeyValuePair<string, string>("to", NormalizePhone(phone)),
                new KeyValuePair<string, string>("document", documentUrl),
                new KeyValuePair<string, string>("filename", fileName),
                new KeyValuePair<string, string>("caption", fileName)
            });

            await PostAsync(url, form);
        }

        private async Task PostAsync(string url, FormUrlEncodedContent form)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(url, form);
            }
            catch (Exception ex)
            {
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status502BadGateway,
                    ex.Message);
            }

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status502BadGateway,
                    $"{response.StatusCode}: {result}");
        }

        private static bool IsImage(string? contentType, string? fileName)
        {
            if (!string.IsNullOrWhiteSpace(contentType) &&
                contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return true;

            var extension = Path.GetExtension(fileName)?.TrimStart('.').ToLowerInvariant();
            return extension is "jpg" or "jpeg" or "png" or "gif" or "webp" or "bmp";
        }

        private static string NormalizePhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.StartsWith("0") && digits.Length > 1 ? digits.TrimStart('0') : digits;
        }
    }
}
