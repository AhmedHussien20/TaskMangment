using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class TaskCommentScopeExtensions
    {
        public static IQueryable<TaskComment> ApplyEmployeeScope( this IQueryable<TaskComment> query,IQueryable<int> scopedEmployeeIds)
        {
            return query.Where(c =>
                c.Task.Assignments.Any(a =>
                    a.IsActive &&
                    scopedEmployeeIds.Contains(a.EmployeeId)));
        }
    }

}
