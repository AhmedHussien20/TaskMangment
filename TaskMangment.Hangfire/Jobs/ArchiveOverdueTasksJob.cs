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
                 .Include(t => t.Assignments)
                .Where(t => t.DueDate < DateTime.UtcNow
                            && t.Status != WorkTaskStatus.Closed
                            && t.Status != WorkTaskStatus.Archived
                            && t.Status != WorkTaskStatus.AutoClose)
                .ToListAsync();

            if (!overdueTasks.Any()) return;

            foreach (var task in overdueTasks)
            {
                var approvedExtension = await _db.TaskExtensionRequests
                    .Where(r => r.TaskId == task.Id
                         && r.Status == ExtensionRequestStatus.Approved
                         && r.NewDueDate > DateTime.UtcNow)
                    .FirstOrDefaultAsync();


                if (approvedExtension != null)
                {
                    continue;
                }

                task.Status = WorkTaskStatus.AutoClose;

                foreach (var assignment in task.Assignments)
                {
                    var discount = new Discount
                    {
                        TaskId = task.Id,
                        EmployeeId = assignment.EmployeeId,
                        Reason = "add discount on auto close task",
                        Amount = task.PenaltyOnAutoClose,
                        CreatedByEmployeeId = 0, 
                        AutoDiscount = true,
                        CreatedDate = DateTime.UtcNow,
                        discountType = DiscountType.AutoCloseTaskDiscount
                        
                    };

                    await _db.Discounts.AddAsync(discount);
                }

                //add check on manager on comment and warning
                if (task.CreatedByEmployeeId.HasValue)
                {
                    var creatorId = task.CreatedByEmployeeId.Value;

                    var creatorCommented = await _db.TaskComments.AnyAsync(c =>
                        c.TaskId == task.Id &&
                        c.EmployeeId == creatorId);

                    var creatorAddedPenalty = await _db.Discounts.AnyAsync(d =>
                        d.TaskId == task.Id &&
                        d.CreatedByEmployeeId == creatorId &&
                        !d.AutoDiscount);

                    var creatorAddedWarning = await _db.Warnings.AnyAsync(w =>
                        w.TaskId == task.Id &&
                        w.CreatedBy == creatorId);

                    var managerDidSomething = creatorCommented || creatorAddedPenalty ||creatorAddedWarning;

                    if (!managerDidSomething)
                    {
                        var managerDiscount = new Discount
                        {
                            TaskId = task.Id,
                            EmployeeId = creatorId,
                            Reason = "manager negligence on overdue task",
                            Amount = task.PenaltyOnAutoClose,
                            CreatedByEmployeeId = 0,
                            AutoDiscount = true,
                            CreatedDate = DateTime.UtcNow,
                            discountType = DiscountType.AutoCloseTaskDiscount
                        };

                        await _db.Discounts.AddAsync(managerDiscount);
                    }
                }

            }

            await _db.SaveChangesAsync();
        }
    }
}
