using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class BranchScopeExtensions
    {
        /// <summary>
        /// Filters to managed branches when BranchIds is set.
        /// When BranchIds is empty, does not widen to the whole company — callers must
        /// apply own-branch / company-wide rules themselves (see BranchService.GetAllAsync).
        /// </summary>
        public static IQueryable<Branch> ApplyAccessScope(
            this IQueryable<Branch> query,
            UserAccessContext access)
        {
            if (access.BranchIds != null && access.BranchIds.Any())
                return query.Where(b => access.BranchIds.Contains(b.Id));

            if (access.OwnBranchId is int ownBranchId)
                return query.Where(b => b.Id == ownBranchId);

            // No managed/home branch → empty (never return all company branches by default).
            return query.Where(b => false);
        }
    }
}
