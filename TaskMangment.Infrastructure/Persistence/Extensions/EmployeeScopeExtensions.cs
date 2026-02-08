using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class EmployeeScopeExtensions
    {
        public static IQueryable<Employee> ApplyAccessScope(this IQueryable<Employee> query,UserAccessContext access)
        {
            if (access.BranchIds.Any())
            {
                query = query.Where(e =>
                    e.BranchId.HasValue &&
                    access.BranchIds.Contains(e.BranchId.Value));
            }

            if (access.FunctionCodes.Any())
            {
                query = query.Where(e =>
                    access.FunctionCodes.Contains(e.FunctionCode));
            }

            return query;
        }
    }


}
