using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class ManagerBranchesScopeExtensions
    {
        /// <summary>
        /// Coverage that actually applies: active link on an active, non-deleted branch.
        /// Inactive ManagerBranches or inactive/deleted branches are out of scope.
        /// </summary>
        public static IQueryable<ManagerBranches> WhereEffectiveCoverage(this IQueryable<ManagerBranches> query)
            => query.Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.Branch != null &&
                !x.Branch.IsDeleted &&
                x.Branch.IsActive);
    }
}
