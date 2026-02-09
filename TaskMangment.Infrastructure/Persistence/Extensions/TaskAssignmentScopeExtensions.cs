using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class TaskAssignmentScopeExtensions
    {
        public static IQueryable<TaskAssignment> ApplyEmployeeScope(this IQueryable<TaskAssignment> query,IQueryable<int> scopedEmployeeIds)
        {
            return query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));
        }
    }

}
