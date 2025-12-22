using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly AppDbContext _db;

        public EmailQueueService(AppDbContext db)
        {
            _db = db;
        }

        public async Task QueueAsync(int userId, string message, string templateKey)
        {
            //var email = new EmailQueue
            //{
            //    ToEmail = "USER_EMAIL_FROM_DB",
            //    TemplateKey = templateKey,
            //    ReferenceType = ReferenceType.Task,
            //    ReferenceId = 0,
            //    ScheduledAt = DateTime.UtcNow,
            //    Status = EmailStatus.Pending
            //};

            //await _db.EmailQueue.AddAsync(email);
            //await _db.SaveChangesAsync();
        }
    }
}
