using Microsoft.EntityFrameworkCore;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    public class PenaltyForMissingCommentsJob
    {
        private readonly AppDbContext _db;

        public PenaltyForMissingCommentsJob(AppDbContext db)
        {
            _db = db;
        }

        public async Task ExecuteAsync()
        {
            var today = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Arab Standard Time").Date;
            var yesterday = today.AddDays(-1);

            var tasksWithCommentPeriod = await _db.Tasks
                .Include(t => t.Assignments)
                .Where(t => t.CommentAllowPeriodDays != null)
                .ToListAsync();

            foreach (var task in tasksWithCommentPeriod)
            {
                foreach (var assignment in task.Assignments)
                {
                    var employeeId = assignment.EmployeeId;
                    var periodDays = (int)task.CommentAllowPeriodDays!.Value;

                    var lastComment = await _db.TaskComments
                        .Where(c => c.TaskId == task.Id && c.EmployeeId == employeeId)
                        .OrderByDescending(c => c.CreatedDate)
                        .FirstOrDefaultAsync();

                    var shouldHaveComment = lastComment == null || lastComment.CreatedDate.Date.AddDays(periodDays) <= yesterday;

                    if (!shouldHaveComment)
                        continue; 

                    var hasLeave = await _db.Leaves
                        .Where(l => l.EmployeeId == employeeId &&
                                    l.StartDate.Date <= yesterday &&
                                    l.EndDate.Date >= yesterday)
                        .AnyAsync();

                    if (hasLeave)
                        continue; 

                    if (task.PenaltyOnStopComment > 0)
                    {
                        var discount = new Discount
                        {
                            TaskId = task.Id,
                            EmployeeId = employeeId,
                            Amount = task.PenaltyOnStopComment,
                            Reason = "discount on not commenting",
                            CreatedByEmployeeId = 0,
                            AutoDiscount = true,
                            CreatedDate = DateTime.UtcNow,
                            discountType = DiscountType.StopCommentDiscount
                        };

                        await _db.Discounts.AddAsync(discount);
                    }
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
