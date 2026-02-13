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
            var targetEmployeeId = request.TargetEmployeeId.GetValueOrDefault(currentEmployeeId);

            if (request.Direction.HasValue)
            {
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
                query = query.Where(t =>
                    t.Assignments.Any(a => a.EmployeeId == currentEmployeeId && a.IsActive) ||
                    t.CreatedByEmployeeId == currentEmployeeId);
            }

            if (request.StatusId.HasValue)
                query = query.Where(t => (int)t.Status == request.StatusId.Value);

            if (request.EmployeeIds != null && request.EmployeeIds.Any())
            {
                query = query.Where(t =>
                    t.Assignments.Any(a => a.IsActive && request.EmployeeIds.Contains(a.EmployeeId)));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.Trim();
                query = query.Where(t => t.Title.Contains(key));
            }

            if (request.PriorityId.HasValue)
                query = query.Where(t => (int)t.Priority == request.PriorityId.Value);

            if (request.CreatedFrom.HasValue)
                query = query.Where(t => t.CreatedDate >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(t => t.CreatedDate <= request.CreatedTo.Value);

            if (request.DueFrom.HasValue)
                query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= request.DueFrom.Value);

            if (request.DueTo.HasValue)
                query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= request.DueTo.Value);

            return query;
        }
    }

}
