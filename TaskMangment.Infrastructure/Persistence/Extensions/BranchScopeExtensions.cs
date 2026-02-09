using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class BranchScopeExtensions
    {
        public static IQueryable<Branch> ApplyAccessScope(this IQueryable<Branch> query,IQueryable<Employee> employeesQuery,UserAccessContext access)
        {
            if (access.BranchIds != null && access.BranchIds.Any())
            {
                query = query.Where(b => access.BranchIds.Contains(b.Id));
                return query;
            }

            if (access.FunctionCodes != null && access.FunctionCodes.Any())
            {
                query = query.Where(b =>
                    employeesQuery.Any(e =>
                        e.BranchId.HasValue &&
                        e.BranchId.Value == b.Id &&
                        access.FunctionCodes.Contains(e.FunctionCode)
                    )
                );
            }

            return query;
        }
    }
}
