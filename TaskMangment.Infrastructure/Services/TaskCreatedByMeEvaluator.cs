using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    /// <summary>
    /// Shared CreatedByMe rules for task UI and close/extension review authorization.
    /// </summary>
    public class TaskCreatedByMeEvaluator
    {
        private readonly IAccessScopeResolver _scopeResolver;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<TaskAssignment> _assignmentRepo;

        public TaskCreatedByMeEvaluator(
            IAccessScopeResolver scopeResolver,
            IRepository<Employee> employeeRepo,
            IRepository<TaskAssignment> assignmentRepo)
        {
            _scopeResolver = scopeResolver;
            _employeeRepo = employeeRepo;
            _assignmentRepo = assignmentRepo;
        }

        /// <summary>
        /// True when user created/assigned the task, or (not an assignee) creator/assigner is under
        /// their <b>managed</b> coverage (ManagerScoped / CompanyWide).
        /// OwnBranch (same home branch) does NOT count — peers/bosses in the same branch are not "my" tasks.
        /// False when the user is only an assignee of a task assigned by someone else.
        /// </summary>
        public static bool Resolve(
            int employeeId,
            WorkTask task,
            IReadOnlySet<int> scopedCreatorOrAssignerIds,
            ResolvedAccessScope scope,
            bool isActiveAssignee)
        {
            if (IsCreatorOrAssigner(employeeId, task))
                return true;

            // Assigned to me from another person → never creator-side.
            if (isActiveAssignee)
                return false;

            // SelfOnly / OwnBranch: visibility may be broader, but creator-side only for literal owner.
            if (!AllowsScopeInferredCreatorSide(scope.Kind))
                return false;

            var creatorInScope = task.CreatedByEmployeeId.HasValue &&
                scopedCreatorOrAssignerIds.Contains(task.CreatedByEmployeeId.Value);
            var assignerInScope = task.AssignedByEmployeeId.HasValue &&
                scopedCreatorOrAssignerIds.Contains(task.AssignedByEmployeeId.Value);

            return creatorInScope || assignerInScope;
        }

        /// <summary>
        /// Scope-inferred creator-side only for real coverage (managed branches/types or company-wide).
        /// OwnBranch = same home branch peers/bosses — not subordinates.
        /// </summary>
        public static bool AllowsScopeInferredCreatorSide(AccessScopeKind kind)
            => kind is AccessScopeKind.ManagerScoped or AccessScopeKind.CompanyWide;

        public static bool IsCreatorOrAssigner(int employeeId, WorkTask task)
            => task.CreatedByEmployeeId == employeeId || task.AssignedByEmployeeId == employeeId;

        public async Task<bool> IsCreatedByMeAsync(WorkTask task, int employeeId)
        {
            if (IsCreatorOrAssigner(employeeId, task))
                return true;

            var isAssignee = await _assignmentRepo
                .GetAll(a => a.TaskId == task.Id && a.IsActive && a.EmployeeId == employeeId)
                .AnyAsync();
            if (isAssignee)
                return false;

            var scope = await _scopeResolver.ResolveAsync(employeeId);
            if (!AllowsScopeInferredCreatorSide(scope.Kind))
                return false;

            var ids = new List<int>();
            if (task.CreatedByEmployeeId is int creatorId && creatorId > 0)
                ids.Add(creatorId);
            if (task.AssignedByEmployeeId is int assignerId && assignerId > 0)
                ids.Add(assignerId);

            if (ids.Count == 0)
                return false;

            return await _scopeResolver
                .FilterEmployees(_employeeRepo.GetAll(e => ids.Contains(e.Id) && !e.IsDeleted), scope)
                .AnyAsync();
        }
    }
}
