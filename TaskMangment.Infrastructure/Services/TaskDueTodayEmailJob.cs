using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskDueTodayEmailJob : ITaskDueTodayEmailJob
    {
        private readonly AppDbContext _db;
        private readonly IEmailQueueService _emailQueue;

        public TaskDueTodayEmailJob(
            AppDbContext db,
            IEmailQueueService emailQueue)
        {
            _db = db;
            _emailQueue = emailQueue;
        }

        public async Task ExecuteAsync()
       {
            var today = DateTime.UtcNow.Date;

            var tasks = await _db.Tasks
                .Where(t =>
                    t.DueDate != null &&
                    t.DueDate.Value.Date == today &&
                    t.Status != WorkTaskStatus.Closed)
                .Select(t => new
                {
                    t.Id,
                    UserIds = t.Assignments
                        .Where(a => a.IsActive)
                        .Select(a => a.EmployeeId)
                        .ToList(),
                       AssignedByEmployeeId = t.AssignedByEmployeeId

                })
                .ToListAsync();

            foreach (var task in tasks)
            {
                var userIds = task.UserIds;

                if (task.AssignedByEmployeeId.HasValue &&
                    !userIds.Contains(task.AssignedByEmployeeId.Value))
                {
                    userIds.Add(task.AssignedByEmployeeId.Value);
                }

                var alreadyQueued = await _db.EmailQueue.AnyAsync(e =>
                    e.TemplateKey == "TaskDueTodayReminder" &&
                    e.ReferenceType == ReferenceType.TaskDueTodayReminder &&
                    e.ReferenceId == task.Id &&
                    e.ScheduledAt.Date == today);

                if (alreadyQueued)
                    continue;

                Console.WriteLine($"Queueing Task {task.Id} for users: {string.Join(',', userIds)}");

                await _emailQueue.QueueAsync(new EmailQueueRequest
                {
                    TemplateKey = "TaskDueTodayReminder",
                    ReferenceType = ReferenceType.TaskDueTodayReminder,
                    ReferenceId = task.Id,
                    UserIds = userIds

                });
            }
        }
    }
}
