using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class EmployeeScopeExtensions
    {
        /// <summary>
        /// Union of managed scopes:
        /// - SeesAllTypes + branches: everyone in those branches
        /// - Branches: everyone in managed branches
        /// - Open types: matching type company-wide
        /// - Branch-restricted types: matching type AND actor branch(es) (home + managed)
        /// </summary>
        public static IQueryable<Employee> ApplyAccessScope(this IQueryable<Employee> query, UserAccessContext access)
        {
            var hasBranches = access.BranchIds != null && access.BranchIds.Any();
            var hasTypes = access.EmployeeTypeIds != null && access.EmployeeTypeIds.Any();

            if (access.SeesAllTypesInBranchScope)
            {
                if (hasBranches)
                {
                    var managed = access.BranchIds.ToList();
                    query = query.Where(e =>
                        e.BranchId.HasValue &&
                        managed.Contains(e.BranchId.Value));
                }

                return query;
            }

            var restrictedTypeIds = (access.BranchRestrictedEmployeeTypeIds ?? Array.Empty<int>())
                .Where(id => access.EmployeeTypeIds != null && access.EmployeeTypeIds.Contains(id))
                .Distinct()
                .ToList();
            var openTypeIds = (access.EmployeeTypeIds ?? Array.Empty<int>())
                .Where(id => !restrictedTypeIds.Contains(id))
                .Distinct()
                .ToList();

            var actorBranchIds = (access.BranchIds ?? Array.Empty<int>()).ToList();
            if (access.OwnBranchId.HasValue && !actorBranchIds.Contains(access.OwnBranchId.Value))
                actorBranchIds.Add(access.OwnBranchId.Value);

            if (!hasBranches && openTypeIds.Count == 0 && restrictedTypeIds.Count == 0)
                return query;

            IQueryable<Employee>? union = null;

            void Or(IQueryable<Employee> part)
            {
                union = union == null ? part : union.Union(part);
            }

            if (hasBranches)
            {
                var managed = access.BranchIds.ToList();
                Or(query.Where(e => e.BranchId.HasValue && managed.Contains(e.BranchId.Value)));
            }

            if (openTypeIds.Count > 0)
            {
                var openIds = openTypeIds;
                Or(query.Where(e => openIds.Contains(e.EmployeeTypeId)));
            }

            if (restrictedTypeIds.Count > 0 && actorBranchIds.Count > 0)
            {
                var restrictedIds = restrictedTypeIds;
                var actorBranches = actorBranchIds;
                Or(query.Where(e =>
                    e.BranchId.HasValue &&
                    actorBranches.Contains(e.BranchId.Value) &&
                    restrictedIds.Contains(e.EmployeeTypeId)));
            }

            return union ?? query;
        }
    }
}
