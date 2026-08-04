namespace TaskMangment.Application.Interfaces.Services
{
    public enum NotificationEventKind
    {
        EmployeeActivity = 0,
        BranchManagerActivity = 1,
        AreaManagerActivity = 2,
        Escalation = 3
    }

    public interface IOrgManagerResolver
    {
        /// <summary>Operational managers for an employee's branch/type (org links, not role level).</summary>
        Task<IReadOnlyList<int>> GetOperationalManagersAsync(int employeeId);

        /// <summary>Escalation recipients for an actor based on org links + RECEIVE_ORG_ESCALATIONS.</summary>
        Task<IReadOnlyList<int>> GetEscalationRecipientsAsync(int actorId, NotificationEventKind eventKind);

        /// <summary>True if target is an org manager over the actor's branch/area.</summary>
        Task<bool> IsOrgManagerOverAsync(int actorId, int targetEmployeeId);

        /// <summary>
        /// Inverse of notification recipients: employees whose RoleNotificationSource graph
        /// would notify this listener (same Branch / Area / Company rules).
        /// </summary>
        /// <param name="listenerEmployeeId">The listener employee.</param>
        /// <param name="onlyListenerRoleId">
        /// When set, only that listening role's notify-from graph is used
        /// (ignores other roles the employee may also hold).
        /// </param>
        Task<IReadOnlyList<int>> GetListenableSubjectIdsAsync(
            int listenerEmployeeId,
            int? onlyListenerRoleId = null);
    }
}
