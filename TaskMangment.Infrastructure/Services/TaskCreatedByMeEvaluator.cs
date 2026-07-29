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
        /// True when user created/assigned the task, or (not an assignee) creator/assigner is in their scope.
        /// False when the user is only an assignee of a task assigned by someone else.
        /// </summary>
        public static bool Resolve(
            int employeeId,
            WorkTask task,
            IReadOnlySet<int> scopedCreatorOrAssignerIds,
            ResolvedAccessScope scope,
            bool isActiveAssignee)
        {
            if (task.CreatedByEmployeeId == employeeId || task.AssignedByEmployeeId == employeeId)
                return true;

            // Assigned to me from another person → never creator-side.
            if (isActiveAssignee)
                return false;

            if (scope.Kind == AccessScopeKind.SelfOnly)
                return false;

            var creatorInScope = task.CreatedByEmployeeId.HasValue &&
                scopedCreatorOrAssignerIds.Contains(task.CreatedByEmployeeId.Value);
            var assignerInScope = task.AssignedByEmployeeId.HasValue &&
                scopedCreatorOrAssignerIds.Contains(task.AssignedByEmployeeId.Value);

            return creatorInScope || assignerInScope;
        }

        public async Task<bool> IsCreatedByMeAsync(WorkTask task, int employeeId)
        {
            if (task.CreatedByEmployeeId == employeeId || task.AssignedByEmployeeId == employeeId)
                return true;

            var isAssignee = await _assignmentRepo
                .GetAll(a => a.TaskId == task.Id && a.IsActive && a.EmployeeId == employeeId)
                .AnyAsync();
            if (isAssignee)
                return false;

            var scope = await _scopeResolver.ResolveAsync(employeeId);
            if (scope.Kind == AccessScopeKind.SelfOnly)
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
