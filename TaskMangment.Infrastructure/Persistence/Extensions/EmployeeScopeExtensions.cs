using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class EmployeeScopeExtensions
    {
        /// <summary>
        /// Operations (SeesAllTypesInBranchScope): filter by branches only (all types in those branches).
        /// Other types: filter by employee type across all branches (no branch list).
        /// </summary>
        public static IQueryable<Employee> ApplyAccessScope(this IQueryable<Employee> query, UserAccessContext access)
        {
            if (access.SeesAllTypesInBranchScope)
            {
                if (access.BranchIds.Any())
                {
                    query = query.Where(e =>
                        e.BranchId.HasValue &&
                        access.BranchIds.Contains(e.BranchId.Value));
                }

                return query;
            }

            if (access.EmployeeTypeIds.Any())
            {
                query = query.Where(e => access.EmployeeTypeIds.Contains(e.EmployeeTypeId));
            }
            else if (access.BranchIds.Any())
            {
                query = query.Where(e =>
                    e.BranchId.HasValue &&
                    access.BranchIds.Contains(e.BranchId.Value));
            }

            return query;
        }
    }
}
