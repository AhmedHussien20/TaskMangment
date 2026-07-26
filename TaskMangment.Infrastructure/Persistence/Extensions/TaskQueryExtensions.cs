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
                // لو فيه TargetEmployeeId → فلتر عليه
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

            // Search across visible task columns (EF-translatable only — no enum/int.ToString())
            if (!string.IsNullOrWhiteSpace(request.searchKey))
                query = query.ApplyTaskSearch(request.searchKey);

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

        /// <summary>
        /// Text by title/description/names/id. Avoids ToString() on enums/ints (not EF-translatable).
        /// </summary>
        public static IQueryable<WorkTask> ApplyTaskSearch(this IQueryable<WorkTask> query, string? searchKey)
        {
            if (string.IsNullOrWhiteSpace(searchKey))
                return query;

            var key = searchKey.Trim();
            int? idMatch = int.TryParse(key, out var parsedId) ? parsedId : null;
            int? statusMatch = int.TryParse(key, out var parsedStatus) ? parsedStatus : null;
            int? priorityMatch = int.TryParse(key, out var parsedPriority) ? parsedPriority : null;

            return query.Where(t =>
                (idMatch.HasValue && t.Id == idMatch.Value) ||
                t.Title.Contains(key) ||
                (t.Description != null && t.Description.Contains(key)) ||
                (t.AssignedBy != null && t.AssignedBy.FullName.Contains(key)) ||
                (t.CreatedBy != null && t.CreatedBy.FullName.Contains(key)) ||
                (statusMatch.HasValue && (int)t.Status == statusMatch.Value) ||
                (priorityMatch.HasValue && (int)t.Priority == priorityMatch.Value) ||
                t.Assignments.Any(a => a.IsActive && a.Employee != null && a.Employee.FullName.Contains(key)));
        }
    }

}
