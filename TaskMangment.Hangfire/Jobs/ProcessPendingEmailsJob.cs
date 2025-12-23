using Microsoft.EntityFrameworkCore; 
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

        public ProcessPendingEmailsJob(AppDbContext db,IEmailService emailService,IEmailTemplateRenderer renderer)
        {
            _db = db;
            _emailService = emailService;
            _renderer = renderer;
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

                    await _emailService.SendEmailAsync(
                        email.ToEmail,
                        rendered.Subject,
                        rendered.Body);

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
