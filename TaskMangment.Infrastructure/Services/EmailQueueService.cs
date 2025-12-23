using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly AppDbContext _db;
        private readonly IRepository<Employee> _employeeRepo;

        public EmailQueueService(
            AppDbContext db,
            IRepository<Employee> employeeRepo)
        {
            _db = db;
            _employeeRepo = employeeRepo;
        }

        public async Task QueueAsync(EmailQueueRequest request)
        {
            var scheduledAt = request.ScheduledAt ?? DateTime.UtcNow;
            scheduledAt.AddMinutes(5);
            foreach (var userId in request.UserIds)
            {
                var employee = await _employeeRepo.GetByIDAsync(userId);
                if (employee == null || string.IsNullOrWhiteSpace(employee.Email))
                    continue;

                var email = new EmailQueue
                {
                    UserId = userId,
                    ToEmail = employee.Email,
                    TemplateKey = request.TemplateKey,
                    ReferenceType = request.ReferenceType,
                    ReferenceId = request.ReferenceId,
                    ScheduledAt = scheduledAt,
                    Status = EmailStatus.Pending
                };

                await _db.EmailQueue.AddAsync(email);
            }

            await _db.SaveChangesAsync();
        }
    }

}
