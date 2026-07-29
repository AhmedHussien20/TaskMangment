using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    public class ProcessPendingEmailsJob
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateRenderer _renderer;
        private readonly IOfferSendService _offerPdfService;
        private readonly IBlobStorageService _blobStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProcessPendingEmailsJob> _logger;

        public ProcessPendingEmailsJob(
            AppDbContext db,
            IEmailService emailService,
            IEmailTemplateRenderer renderer,
            IOfferSendService offerPdfService,
            IBlobStorageService blobStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<ProcessPendingEmailsJob> logger)
        {
            _db = db;
            _emailService = emailService;
            _renderer = renderer;
            _offerPdfService = offerPdfService;
            _blobStorage = blobStorage;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var emails = await _db.EmailQueue
                .Where(x => x.Status == EmailStatus.Pending && x.ScheduledAt <= DateTime.UtcNow)
                .Take(50)
                .ToListAsync();

            foreach (var email in emails)
            {
                try
                {
                    email.Status = EmailStatus.Processing;

                    if (email.ForAll)
                    {
                        await SendToAllEmployeesAsync(email);
                    }
                    else
                    {
                        await SendSingleQueuedEmailAsync(email);
                    }

                    email.Status = EmailStatus.Sent;
                    email.SentAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    email.Status = EmailStatus.Failed;
                    email.RetryCount++;
                    email.ErrorMessage = ex.Message;
                    _logger.LogError(
                        ex,
                        "Failed processing email queue item {EmailId} template {TemplateKey}",
                        email.Id,
                        email.TemplateKey);
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task SendSingleQueuedEmailAsync(EmailQueue email)
        {
            if (email.TemplateKey == "DeveloperErrorAlert")
            {
                var raw = email.ErrorMessage ?? "";
                var splitIndex = raw.IndexOf("\n\n", StringComparison.Ordinal);

                var subject = splitIndex > -1 ? raw.Substring(0, splitIndex) : "Developer Error Alert";
                var body = splitIndex > -1 ? raw.Substring(splitIndex + 2) : raw;

                await _emailService.SendEmailAsync(email.ToEmail!, subject, body);
                return;
            }

            var rendered = await _renderer.RenderAsync(
                email.TemplateKey,
                email.ReferenceType,
                email.ReferenceId,
                email.UserId,
                ParseMetadataTokens(email.MetadataJson));

            var attachments = await BuildAttachmentsAsync(email);

            await _emailService.SendEmailAsync(
                email.ToEmail!,
                rendered.Subject,
                rendered.Body,
                attachments);
        }

        private static IReadOnlyDictionary<string, string>? ParseMetadataTokens(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);
            }
            catch
            {
                return null;
            }
        }

        private async Task SendToAllEmployeesAsync(EmailQueue batchEmail)
        {
            const int chunkSize = 200;
            int lastId = 0;

            var rendered = await _renderer.RenderAsync(
                batchEmail.TemplateKey,
                batchEmail.ReferenceType,
                batchEmail.ReferenceId,
                null);

            var attachments = await BuildAttachmentsAsync(batchEmail);

            while (true)
            {
                var recipients = await _db.Employees
                    .AsNoTracking()
                    .Where(e => e.Id > lastId && !string.IsNullOrEmpty(e.Email))
                    .OrderBy(e => e.Id)
                    .Select(e => new { e.Id, e.Email })
                    .Take(chunkSize)
                    .ToListAsync();

                if (recipients.Count == 0)
                    break;

                foreach (var r in recipients)
                {
                    await _emailService.SendEmailAsync(
                        r.Email!,
                        rendered.Subject,
                        rendered.Body,
                        attachments);

                    lastId = r.Id;
                }
            }
        }

        private async Task<List<EmailAttachment>?> BuildAttachmentsAsync(EmailQueue email)
        {
            if (email.TemplateKey == "OfferSent")
            {
                var pdfBytes = await _offerPdfService.GenerateOfferPdfBytesAsync(email.ReferenceId);
                return new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        Name = $"Offer-{email.ReferenceId}.pdf",
                        ContentBase64 = Convert.ToBase64String(pdfBytes)
                    }
                };
            }

            if (email.TemplateKey == "TaskCommentAdded"
                || email.ReferenceType == ReferenceType.TaskComment)
            {
                return await LoadCommentAttachmentsAsync(email.ReferenceId);
            }

            return null;
        }

        private async Task<List<EmailAttachment>?> LoadCommentAttachmentsAsync(int commentId)
        {
            var files = await _db.Attachments
                .AsNoTracking()
                .Where(a =>
                    a.ReferenceId == commentId &&
                    a.AttachmentType == AttachmentType.Comment &&
                    !a.IsDeleted)
                .Select(a => new { a.FileName, a.FilePath, a.BlobUrl, a.ContentType })
                .ToListAsync();

            if (files.Count == 0)
            {
                _logger.LogInformation("No attachments found for comment {CommentId}", commentId);
                return null;
            }

            var result = new List<EmailAttachment>();

            foreach (var file in files)
            {
                var path = !string.IsNullOrWhiteSpace(file.BlobUrl) ? file.BlobUrl : file.FilePath;
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning(
                        "Comment {CommentId} attachment {FileName} has empty path",
                        commentId,
                        file.FileName);
                    continue;
                }

                var name = string.IsNullOrWhiteSpace(file.FileName)
                    ? $"attachment-{result.Count + 1}{GuessExtension(file.ContentType)}"
                    : file.FileName;

                try
                {
                    // Prefer Azure SDK download (works reliably in Azure App Service).
                    var bytes = await _blobStorage.DownloadBytesAsync(path);
                    if (bytes == null || bytes.Length == 0)
                    {
                        // Fallback: HTTP + SAS (same as local/WhatsApp path).
                        var url = _blobStorage.WithSas(path);
                        if (UriLooksAbsolute(url))
                        {
                            var http = _httpClientFactory.CreateClient();
                            http.Timeout = TimeSpan.FromMinutes(2);
                            using var response = await http.GetAsync(url);
                            if (response.IsSuccessStatusCode)
                                bytes = await response.Content.ReadAsByteArrayAsync();
                        }
                    }

                    if (bytes == null || bytes.Length == 0)
                    {
                        _logger.LogWarning(
                            "Could not download comment {CommentId} attachment {FileName} from {Path}",
                            commentId,
                            file.FileName,
                            path);
                        continue;
                    }

                    result.Add(new EmailAttachment
                    {
                        Name = name,
                        ContentBase64 = Convert.ToBase64String(bytes),
                        Url = _blobStorage.WithSas(path)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Exception downloading comment {CommentId} attachment {FileName}",
                        commentId,
                        file.FileName);
                }
            }

            _logger.LogInformation(
                "Prepared {Count}/{Total} attachment(s) as base64 for comment {CommentId}",
                result.Count,
                files.Count,
                commentId);

            return result.Count > 0 ? result : null;
        }

        private static bool UriLooksAbsolute(string url)
            => Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        private static string GuessExtension(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return ".bin";

            return contentType.ToLowerInvariant() switch
            {
                "application/pdf" => ".pdf",
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "application/msword" => ".doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                "application/vnd.ms-excel" => ".xls",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                _ => ".bin"
            };
        }
    }
}
