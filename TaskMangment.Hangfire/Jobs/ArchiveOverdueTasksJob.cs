using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    public class ArchiveOverdueTasksJob
    {
        private readonly AppDbContext _db;

        public ArchiveOverdueTasksJob(AppDbContext db)
        {
            _db = db;
        }

        public async Task ExecuteAsync()
        {
            var overdueTasks = await _db.Tasks
                .Where(t => t.DueDate < DateTime.UtcNow
                            && t.Status != WorkTaskStatus.Closed
                            && t.Status != WorkTaskStatus.Archived)
                .ToListAsync();

            if (!overdueTasks.Any()) return;

            foreach (var task in overdueTasks)
            {
                task.Status = WorkTaskStatus.Archived;
            }

            await _db.SaveChangesAsync();
        }
    }
}
