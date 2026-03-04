using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class TaskQueryExtensions
    {
        public static IQueryable<WorkTask> ApplyTaskFilters(
            this IQueryable<WorkTask> query,
            TaskRequest request,
            int currentEmployeeId)
        {
            if (request.Direction.HasValue)
            {
                if (request.TargetEmployeeId.HasValue)
                {
                    var targetEmployeeId = request.TargetEmployeeId.Value;

                    if (request.Direction.Value == TaskDirection.Incoming)
                    {
                        query = query.Where(t => t.Assignments.Any(a =>
                            a.IsActive && a.EmployeeId == targetEmployeeId));
                    }
                    else
                    {
                        query = query.Where(t => t.CreatedByEmployeeId == targetEmployeeId);
                    }
                }
                else
                {
                    // مفيش TargetEmployeeId → متفلترش على موظف
                    // بس فلتر على الاتجاه فقط

                    if (request.Direction.Value == TaskDirection.Incoming)
                    {
                        query = query.Where(t =>
                            t.Assignments.Any(a => a.IsActive));
                    }
                    else
                    {
                        query = query.Where(t => t.CreatedByEmployeeId != 0);
                    }
                }
            }

            // Status
            if (request.StatusId.HasValue)
                query = query.Where(t => (int)t.Status == request.StatusId.Value);

            // EmployeeIds multi-select
            if (request.EmployeeIds != null && request.EmployeeIds.Any())
            {
                query = query.Where(t =>
                    t.Assignments.Any(a => a.IsActive && request.EmployeeIds.Contains(a.EmployeeId)));
            }

            // Search
            if (!string.IsNullOrWhiteSpace(request.searchKey))
            {
                var key = request.searchKey.Trim();
                query = query.Where(t => t.Title.Contains(key));
            }

            // Priority
            if (request.PriorityId.HasValue)
                query = query.Where(t => (int)t.Priority == request.PriorityId.Value);

            // Created range
            if (request.CreatedFrom.HasValue)
                query = query.Where(t => t.CreatedDate >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(t => t.CreatedDate <= request.CreatedTo.Value);

            // Due range
            if (request.DueFrom.HasValue)
                query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= request.DueFrom.Value);

            if (request.DueTo.HasValue)
                query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= request.DueTo.Value);

            return query;
        }
    }

}
