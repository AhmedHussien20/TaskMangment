using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class EmployeeScopeExtensions
    {
        /// <summary>
        /// Union of managed scopes:
        /// - SeesAllTypes + branches: everyone in those branches
        /// - Branches + types: in managed branches OR matching type (company-wide)
        /// - Types only: matching type company-wide
        /// - Branches only: everyone in those branches
        /// </summary>
        public static IQueryable<Employee> ApplyAccessScope(this IQueryable<Employee> query, UserAccessContext access)
        {
            var hasBranches = access.BranchIds != null && access.BranchIds.Any();
            var hasTypes = access.EmployeeTypeIds != null && access.EmployeeTypeIds.Any();

            if (access.SeesAllTypesInBranchScope)
            {
                if (hasBranches)
                {
                    query = query.Where(e =>
                        e.BranchId.HasValue &&
                        access.BranchIds.Contains(e.BranchId.Value));
                }

                return query;
            }

            if (hasBranches && hasTypes)
            {
                query = query.Where(e =>
                    (e.BranchId.HasValue && access.BranchIds.Contains(e.BranchId.Value)) ||
                    access.EmployeeTypeIds.Contains(e.EmployeeTypeId));
            }
            else if (hasTypes)
            {
                query = query.Where(e => access.EmployeeTypeIds.Contains(e.EmployeeTypeId));
            }
            else if (hasBranches)
            {
                query = query.Where(e =>
                    e.BranchId.HasValue &&
                    access.BranchIds.Contains(e.BranchId.Value));
            }

            return query;
        }
    }
}
