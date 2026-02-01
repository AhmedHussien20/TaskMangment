using Microsoft.EntityFrameworkCore;
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


        public ProcessPendingEmailsJob(AppDbContext db, IEmailService emailService, IEmailTemplateRenderer renderer, IOfferSendService offerPdfService)
        {
            _db = db;
            _emailService = emailService;
            _renderer = renderer;
            _offerPdfService = offerPdfService;
        }

        public async Task ExecuteAsync()
        {
            var emails = await _db.EmailQueue
                .Where(x =>
                    x.Status == EmailStatus.Pending &&
                    x.ScheduledAt <= DateTime.UtcNow)
                .Take(50)
                .ToListAsync();

            foreach (var email in emails)
            {
                try
                {
                    email.Status = EmailStatus.Processing;

                    var rendered = await _renderer.RenderAsync(
                        email.TemplateKey,
                        email.ReferenceType,
                        email.ReferenceId,
                        email.UserId);

                    if (email.TemplateKey == "OfferSent")
                    {
                        var pdfBytes = await _offerPdfService.GenerateOfferPdfBytesAsync(email.ReferenceId);
                        var attachments = new List<EmailAttachment>
                           {
                                 new EmailAttachment{
                                     Name = $"Offer-{email.ReferenceId}.pdf",
                                     ContentBase64 = Convert.ToBase64String(pdfBytes)
                                 }
                        };
                        await _emailService.SendEmailAsync(
                        email.ToEmail,
                        rendered.Subject,
                        rendered.Body,
                        attachments);
                    }
                    else
                    {
                        await _emailService.SendEmailAsync(
                        email.ToEmail,
                        rendered.Subject,
                        rendered.Body);
                    }

                    email.Status = EmailStatus.Sent;
                    email.SentAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    email.Status = EmailStatus.Failed;
                    email.RetryCount++;
                    email.ErrorMessage = ex.Message;
                }
            }

            await _db.SaveChangesAsync();
        }
    }


}
