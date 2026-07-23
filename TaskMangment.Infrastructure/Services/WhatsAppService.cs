using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private const int MaxAttachmentBytes = 15 * 1024 * 1024; // UltraMsg practical limit
        private const string AutoSystemDisclaimer =
            "تم إرسال هذه الرسالة تلقائيًا من النظام لإشعارك بالمهمة أو التنبيه. يُرجى عدم الرد على هذه الرسالة، واستخدام النظام لمتابعة المهمة أو إضافة أي تعليق. شكرًا لك.";

        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            HttpClient httpClient,
            IOptions<WhatsAppSettings> options,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendNotificationAsync(
            string phone,
            string userName,
            string message,
            IReadOnlyList<WhatsAppAttachment>? attachments = null)
        {
            EnsureConfigured(phone);

            var text = FormatTextMessage(userName, message);
            var validAttachments = (attachments ?? Array.Empty<WhatsAppAttachment>())
                .Where(a => !string.IsNullOrWhiteSpace(a.Url))
                .ToList();

            // No files → normal text message
            if (validAttachments.Count == 0)
            {
                await SendChatAsync(phone, text);
                return;
            }

            // Has files → put the comment text as caption on the media (same WhatsApp message).
            // WhatsApp allows only one file per message; first file gets the full text caption.
            for (var i = 0; i < validAttachments.Count; i++)
            {
                var attachment = validAttachments[i];
                try
                {
                    var mediaBase64 = await DownloadAsBase64Async(attachment.Url);
                    var safeFileName = SanitizeFileName(attachment.FileName);
                    // Caption max length for UltraMsg is 1024.
                    var caption = TruncateCaption(i == 0 ? text : safeFileName);

                    if (IsImage(attachment.ContentType, attachment.FileName))
                        await SendImageAsync(phone, mediaBase64, caption);
                    else
                        await SendDocumentAsync(phone, mediaBase64, safeFileName, caption);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send WhatsApp attachment {FileName} to {Phone}",
                        attachment.FileName,
                        phone);
                    throw;
                }
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
            var withDisclaimer = string.IsNullOrWhiteSpace(body)
                ? AutoSystemDisclaimer
                : $"{body}\n\n{AutoSystemDisclaimer}";

            if (string.IsNullOrWhiteSpace(userName))
                return withDisclaimer;

            return $"{userName.Trim()}\n\n{withDisclaimer}";
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

        private async Task SendImageAsync(string phone, string imagePayload, string caption)
        {
            var url = $"https://api.ultramsg.com/{_settings.InstanceId}/messages/image";
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", _settings.Token),
                new KeyValuePair<string, string>("to", NormalizePhone(phone)),
                new KeyValuePair<string, string>("image", imagePayload),
                new KeyValuePair<string, string>("caption", caption)
            });

            await PostAsync(url, form);
        }

        private async Task SendDocumentAsync(string phone, string documentPayload, string fileName, string caption)
        {
            var url = $"https://api.ultramsg.com/{_settings.InstanceId}/messages/document";
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", _settings.Token),
                new KeyValuePair<string, string>("to", NormalizePhone(phone)),
                new KeyValuePair<string, string>("document", documentPayload),
                new KeyValuePair<string, string>("filename", fileName),
                new KeyValuePair<string, string>("caption", caption)
            });

            await PostAsync(url, form);
        }

        private async Task<string> DownloadAsBase64Async(string fileUrl)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(fileUrl);
            }
            catch (Exception ex)
            {
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status502BadGateway,
                    $"Failed to download attachment for WhatsApp: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status502BadGateway,
                    $"Failed to download attachment ({response.StatusCode}): {errorBody}");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0)
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status400BadRequest,
                    "Attachment file is empty");

            if (bytes.Length > MaxAttachmentBytes)
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status400BadRequest,
                    $"Attachment exceeds WhatsApp size limit ({MaxAttachmentBytes / (1024 * 1024)} MB)");

            return Convert.ToBase64String(bytes);
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

            // UltraMsg often returns HTTP 200 with an error JSON body.
            if (result.Contains("\"error\"", StringComparison.OrdinalIgnoreCase) &&
                !result.Contains("\"sent\"", StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException(
                    ErrorCodes.WhatsAppSendFailed,
                    StatusCodes.Status502BadGateway,
                    result);
            }
        }

        private static bool IsImage(string? contentType, string? fileName)
        {
            if (!string.IsNullOrWhiteSpace(contentType) &&
                contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return true;

            var extension = Path.GetExtension(fileName)?.TrimStart('.').ToLowerInvariant();
            return extension is "jpg" or "jpeg" or "png" or "gif" or "webp" or "bmp";
        }

        private static string SanitizeFileName(string? fileName)
        {
            var name = Path.GetFileName(string.IsNullOrWhiteSpace(fileName) ? "file" : fileName.Trim());
            var extension = Path.GetExtension(name);
            var baseName = Path.GetFileNameWithoutExtension(name);

            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "file";

            // UltraMsg is picky about filename charset — keep ASCII-safe names.
            var safeBase = new string(baseName
                .Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_')
                .ToArray());

            if (string.IsNullOrWhiteSpace(safeBase))
                safeBase = "file";

            if (safeBase.Length > 80)
                safeBase = safeBase[..80];

            return string.IsNullOrWhiteSpace(extension) ? safeBase : $"{safeBase}{extension}";
        }

        private static string TruncateCaption(string caption)
        {
            const int maxLength = 1024;
            if (string.IsNullOrEmpty(caption))
                return "-";

            return caption.Length <= maxLength
                ? caption
                : caption[..(maxLength - 1)] + "…";
        }

        private static string NormalizePhone(string phone)
        {
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.StartsWith("0") && digits.Length > 1 ? digits.TrimStart('0') : digits;
        }
    }
}
