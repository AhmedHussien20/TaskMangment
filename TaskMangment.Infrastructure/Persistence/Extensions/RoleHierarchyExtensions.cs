using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Persistence.Extensions
{
    public static class RoleHierarchyExtensions
    {
        /// <summary>
        /// Legacy level-based filter. Prefer permission + access scope + org manager exclusion.
        /// Kept temporarily for unmigrated call sites.
        /// </summary>
        [Obsolete("Use permission + access scope + org manager exclusion instead of Role.Level.")]
        public static IQueryable<Employee> ApplyRoleHierarchy(this IQueryable<Employee> query, int maxRoleLevel)
        {
            return query.Where(e => !e.EmployeeRoles.Any(er => er.IsAssigned &&
            !er.IsDeleted &&
            er.Role != null &&
            er.Role.Level > maxRoleLevel));
        }

        /// <summary>
        /// Excludes employees who are org managers over the actor.
        /// </summary>
        public static IQueryable<Employee> ExcludeOrgManagersOver(
            this IQueryable<Employee> query,
            IEnumerable<int> orgManagerIds)
        {
            var ids = orgManagerIds?.Distinct().ToList() ?? [];
            if (ids.Count == 0)
                return query;

            return query.Where(e => !ids.Contains(e.Id));
        }
    }
}
