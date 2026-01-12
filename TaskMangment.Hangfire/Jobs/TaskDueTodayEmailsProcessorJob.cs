using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Hangfire.Jobs
{
    public class TaskDueTodayEmailsProcessorJob
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateRenderer _renderer;
        private readonly IStringLocalizer<DiscountTypes> _localizer;

        public TaskDueTodayEmailsProcessorJob(
            AppDbContext db,
            IEmailService emailService,
            IEmailTemplateRenderer renderer, IStringLocalizer<DiscountTypes> localizer)
        {
            _db = db;
            _emailService = emailService;
            _renderer = renderer;
            _localizer = localizer;
        }
       
        public async Task ExecuteAsync()
        {
            var emails = await _db.EmailQueue
                .Where(e =>
                    e.Status == EmailStatus.Pending &&
                    e.TemplateKey == "TaskDueTodayReminder" &&
                    e.ScheduledAt <= DateTime.UtcNow)
                .OrderBy(e => e.Id)
                .Take(50)
                .ToListAsync();

            foreach (var email in emails)
                email.Status = EmailStatus.Processing;

            await _db.SaveChangesAsync();

            foreach (var email in emails)
            {
                try
                {
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
