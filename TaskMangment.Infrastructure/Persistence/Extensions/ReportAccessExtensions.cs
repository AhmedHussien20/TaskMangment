using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class ReportAccessExtensions
    {
        public static IQueryable<TaskAssignment> ApplySelfOnlyIfNeeded(this IQueryable<TaskAssignment> query,bool canViewAll, int currentEmployeeId)
        {
            if (!canViewAll)
            {
                query = query.Where(a => a.EmployeeId == currentEmployeeId);
            }

            return query;
        }

        public static IQueryable<TaskComment> ApplySelfOnlyIfNeeded(this IQueryable<TaskComment> query,bool canViewAll, int currentEmployeeId)
        {
            if (!canViewAll)
            {
                query = query.Where(c =>
                    c.Task.Assignments.Any(a =>
                        a.IsActive &&
                        a.EmployeeId == currentEmployeeId));
            }
            return query;
        }
    }

}
