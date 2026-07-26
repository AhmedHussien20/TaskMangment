using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private static readonly HashSet<string> BrevoAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".xlsx", ".xls", ".ods", ".docx", ".docm", ".doc", ".csv", ".pdf", ".txt",
            ".gif", ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".rtf", ".bmp", ".cgm",
            ".css", ".shtml", ".html", ".htm", ".zip", ".xml", ".ppt", ".pptx", ".tar",
            ".ez", ".ics", ".mobi", ".msg", ".pub", ".eps", ".odt", ".mp3", ".m4a",
            ".m4v", ".wma", ".ogg", ".flac", ".wav", ".aif", ".aifc", ".aiff", ".mp4",
            ".mov", ".avi", ".mkv", ".mpeg", ".mpg", ".wmv", ".pkpass", ".xlsm"
        };

        private readonly EmailSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> settings,
            HttpClient httpClient,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, List<EmailAttachment>? attachments = null)
        {
            if (!_settings.SendEnabled)
            {
                _logger.LogInformation(
                    "Email send disabled (EmailSettings.SendEnabled=false). Skipping send to {To}, subject: {Subject}",
                    to,
                    subject);
                return;
            }

            if (string.IsNullOrWhiteSpace(_settings.From))
                throw new Exception("From email is missing in EmailSettings.");

            if (!string.IsNullOrWhiteSpace(_settings.BrevoApiKey))
            {
                await SendViaBrevoApiAsync(to, subject, body, attachments);
                return;
            }

            if (!string.IsNullOrWhiteSpace(_settings.Host))
            {
                await SendViaSmtpAsync(to, subject, body, attachments);
                return;
            }

            throw new Exception("Email is not configured. Set EmailSettings BrevoApiKey or Host/User/Password.");
        }

        private async Task SendViaSmtpAsync(string to, string subject, string body, List<EmailAttachment>? attachments)
        {
            if (string.IsNullOrWhiteSpace(_settings.User) || string.IsNullOrWhiteSpace(_settings.Password))
                throw new Exception("SMTP User/Password are missing in EmailSettings.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var bytes = await ResolveAttachmentBytesAsync(attachment);
                    if (bytes == null || bytes.Length == 0)
                    {
                        _logger.LogWarning(
                            "Skipping empty/unreadable email attachment {FileName} for {To}",
                            attachment.Name,
                            to);
                        continue;
                    }

                    builder.Attachments.Add(SanitizeFileName(attachment.Name), bytes);
                }
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _settings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            var user = _settings.User.Trim();
            var password = _settings.Password.Trim();

            try
            {
                await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                throw new Exception(
                    $"SMTP authentication failed for user '{user}' on {_settings.Host}:{_settings.Port}. " +
                    "Check Brevo SMTP login/password (SMTP & API > SMTP), and Azure App Settings EMAIL_HOST_USER / EMAIL_HOST_PASSWORD if set.",
                    ex);
            }
        }

        private async Task SendViaBrevoApiAsync(string to, string subject, string body, List<EmailAttachment>? attachments)
        {
            // Always send base64 content to Brevo.
            // URL attachments often return 201 then fail delivery when Brevo cannot fetch Azure SAS blobs.
            var brevoAttachments = new List<Dictionary<string, string>>();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var name = SanitizeFileNameForBrevo(attachment.Name);
                    var bytes = await ResolveAttachmentBytesAsync(attachment);
                    if (bytes == null || bytes.Length == 0)
                    {
                        _logger.LogWarning(
                            "Skipping Brevo attachment {FileName} for {To}: could not resolve file bytes",
                            attachment.Name,
                            to);
                        continue;
                    }

                    brevoAttachments.Add(new Dictionary<string, string>
                    {
                        ["content"] = Convert.ToBase64String(bytes),
                        ["name"] = name
                    });
                }
            }

            var payload = new Dictionary<string, object>
            {
                ["sender"] = new Dictionary<string, string>
                {
                    ["email"] = _settings.From,
                    ["name"] = _settings.SenderName ?? _settings.From
                },
                ["to"] = new[]
                {
                    new Dictionary<string, string> { ["email"] = to }
                },
                ["subject"] = subject,
                ["htmlContent"] = body
            };

            if (brevoAttachments.Count > 0)
            {
                payload["attachment"] = brevoAttachments;
                _logger.LogInformation(
                    "Sending Brevo email to {To} with {Count} base64 attachment(s)",
                    to,
                    brevoAttachments.Count);
            }
            else if (attachments != null && attachments.Count > 0)
            {
                _logger.LogWarning(
                    "Email to {To} had {Count} attachment input(s) but none could be resolved; sending text only",
                    to,
                    attachments.Count);
            }

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", _settings.BrevoApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Brevo email send failed: {(int)response.StatusCode} - {responseBody}");
        }

        private async Task<byte[]?> ResolveAttachmentBytesAsync(EmailAttachment attachment)
        {
            if (!string.IsNullOrWhiteSpace(attachment.ContentBase64))
            {
                try
                {
                    return Convert.FromBase64String(attachment.ContentBase64);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Invalid base64 for attachment {FileName}", attachment.Name);
                }
            }

            if (string.IsNullOrWhiteSpace(attachment.Url))
                return null;

            try
            {
                using var response = await _httpClient.GetAsync(attachment.Url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to download attachment {FileName} from URL. Status={StatusCode}",
                        attachment.Name,
                        (int)response.StatusCode);
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download attachment {FileName} from URL", attachment.Name);
                return null;
            }
        }

        private static string SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "attachment.bin";

            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(fileName.Where(ch => !invalid.Contains(ch)).ToArray()).Trim();
            if (string.IsNullOrWhiteSpace(cleaned))
                return "attachment.bin";

            if (!Path.HasExtension(cleaned))
                cleaned += ".bin";

            return cleaned;
        }

        private static string SanitizeFileNameForBrevo(string? fileName)
        {
            var cleaned = SanitizeFileName(fileName);
            var ext = Path.GetExtension(cleaned);

            if (!BrevoAllowedExtensions.Contains(ext))
            {
                var baseName = Path.GetFileNameWithoutExtension(cleaned);
                if (string.IsNullOrWhiteSpace(baseName))
                    baseName = "attachment";
                cleaned = baseName + ".pdf";
            }

            // Keep ASCII-ish name to avoid rare provider delivery issues.
            var safe = new string(cleaned.Select(ch => ch < 128 ? ch : '_').ToArray());
            if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(safe)))
                safe = "attachment" + Path.GetExtension(cleaned);

            return safe;
        }
    }
}
