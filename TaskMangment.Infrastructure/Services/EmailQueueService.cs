using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Entities.MongoDB_Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly IMongoRepository<EmailQueueMongo> _emailQueueRepo;
        private readonly AppDbContext _db;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<Student> _studentRepo;


        public EmailQueueService(
            AppDbContext db,
            IRepository<Employee> employeeRepo,
            IRepository<Student> studentRepo,
            IMongoRepository<EmailQueueMongo> emailQueueRepo)
        {
            _db = db;
            _employeeRepo = employeeRepo;
            _studentRepo = studentRepo;
            _emailQueueRepo = emailQueueRepo;
        }

        public async Task QueueAsync(EmailQueueRequest request)
        {
            var scheduledAt = request.ScheduledAt ?? DateTime.UtcNow;
            scheduledAt = scheduledAt.AddMinutes(5);

            if (request.ForAll)
            {
                await _emailQueueRepo.AddAsync(new EmailQueueMongo
                {
                    ForAll = true,
                    ToEmail = "For all employees",
                    TemplateKey = request.TemplateKey,
                    ReferenceType = request.ReferenceType,
                    ReferenceId = request.ReferenceId,
                    ScheduledAt = scheduledAt,
                    Status = EmailStatus.Pending
                });
                return;
            }

            foreach (var userId in request.UserIds)
            {
                string? email = request.RecipientType switch
                {
                    RecipientType.Employee =>
                        (await _employeeRepo.GetByIDAsync(userId))?.Email,
                    RecipientType.Student =>
                        (await _studentRepo.GetByIDAsync(userId))?.Email,
                    _ => null
                };

                if (string.IsNullOrWhiteSpace(email))
                    continue;

                var emailQueue = new EmailQueueMongo
                {
                    UserId = userId,
                    ToEmail = email,
                    TemplateKey = request.TemplateKey,
                    ReferenceType = request.ReferenceType,
                    ReferenceId = request.ReferenceId,
                    ScheduledAt = scheduledAt,
                    Status = EmailStatus.Pending
                };

                await _emailQueueRepo.AddAsync(emailQueue);
            }
        }

        public async Task QueueDirectAsync(string toEmail, string subject, string htmlBody, DateTime? scheduledAt = null)
        {
            var when = scheduledAt ?? DateTime.UtcNow;

            await _emailQueueRepo.AddAsync(new EmailQueueMongo
            {
                ToEmail = toEmail,
                TemplateKey = "DeveloperErrorAlert",
                ReferenceType = ReferenceType.DevelopmentException,
                ReferenceId = 0,
                ScheduledAt = when,
                Status = EmailStatus.Pending,
                ErrorMessage = subject + "\n\n" + htmlBody
            });
        }

        //public async Task QueueAsync(EmailQueueRequest request)
        //{
        //    var scheduledAt = request.ScheduledAt ?? DateTime.UtcNow;
        //    scheduledAt.AddMinutes(5);

        //    if (request.ForAll)
        //    {
        //        await _db.EmailQueue.AddAsync(new EmailQueue
        //        {
        //            ForAll = true,
        //            ToEmail = "For all employees",
        //            TemplateKey = request.TemplateKey,
        //            ReferenceType = request.ReferenceType,
        //            ReferenceId = request.ReferenceId,
        //            ScheduledAt = scheduledAt,
        //            Status = EmailStatus.Pending
        //        });

        //        await _db.SaveChangesAsync();
        //        return;
        //    }

        //    foreach (var userId in request.UserIds)
        //    {
        //        string? email = request.RecipientType switch
        //        {
        //            RecipientType.Employee =>
        //                (await _employeeRepo.GetByIDAsync(userId))?.Email,

        //            RecipientType.Student =>
        //                (await _studentRepo.GetByIDAsync(userId))?.Email,

        //            _ => null
        //        };

        //        if (string.IsNullOrWhiteSpace(email))
        //            continue;

        //        var emailQueue = new EmailQueue
        //        {
        //            UserId = userId,
        //            ToEmail = email,
        //            TemplateKey = request.TemplateKey,
        //            ReferenceType = request.ReferenceType,
        //            ReferenceId = request.ReferenceId,
        //            ScheduledAt = scheduledAt,
        //            Status = EmailStatus.Pending
        //            //MetadataJson = request.MetadataJson

        //        };

        //        await _db.EmailQueue.AddAsync(emailQueue);
        //    }


        //    await _db.SaveChangesAsync();
        //}

        //public async Task QueueDirectAsync(string toEmail, string subject, string htmlBody, DateTime? scheduledAt = null)
        //{
        //    var when = scheduledAt ?? DateTime.UtcNow;

        //    await _db.EmailQueue.AddAsync(new EmailQueue
        //    {
        //        ToEmail = toEmail,
        //        TemplateKey = "DeveloperErrorAlert",
        //        ReferenceType = ReferenceType.DevelopmentException,
        //        ReferenceId = 0,
        //        ScheduledAt = when,
        //        Status = EmailStatus.Pending,

        //        ErrorMessage = subject + "\n\n" + htmlBody
        //    });

        //    await _db.SaveChangesAsync();
        //}

    }

}
