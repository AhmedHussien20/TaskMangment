namespace TaskMangment.Application.Common.Security
{
    /// <summary>
    /// Canonical permission codes. Roles are only bags of these codes — never gate on role name/level.
    /// </summary>
    public static class PermissionCodes
    {
        // Tasks — CRUD / view
        public const string CreateTask = "CREATE_TASK";
        public const string UpdateTask = "UPDATE_TASK";
        public const string DeleteTask = "DELETE_TASK";
        public const string CommentTask = "COMMENT_TASK";
        public const string ViewOwnTasks = "VIEW_OWN_TASKS";
        public const string ViewScopedTasks = "VIEW_SCOPED_TASKS";
        public const string ViewCompanyTasks = "VIEW_COMPANY_TASKS";

        // Tasks — assignee requests
        public const string RequestTaskClose = "REQUEST_TASK_CLOSE";
        public const string RequestDueDateExtension = "REQUEST_DUE_DATE_EXTENSION";

        // Tasks — creator / scoped-manager actions
        public const string ApproveTaskRequest = "APPROVE_TASK_REQUEST";
        public const string RejectTaskRequest = "REJECT_TASK_REQUEST";
        public const string ArchiveTask = "ARCHIVE_TASK";
        public const string IssueWarning = "ISSUE_WARNING";
        public const string DeleteWarning = "DELETE_WARNING";
        public const string IssuePenalty = "ISSUE_PENALTY";
        public const string DeletePenalty = "DELETE_PENALTY";

        // People / org management
        public const string ViewEmployees = "VIEW_EMPLOYEES";
        public const string CreateEmployee = "CREATE_EMPLOYEE";
        public const string UpdateEmployee = "UPDATE_EMPLOYEE";
        public const string DeleteEmployee = "DELETE_EMPLOYEE";
        public const string EnableEmployee = "ENABLE_EMPLOYEE";
        public const string DisableEmployee = "DISABLE_EMPLOYEE";
        public const string AssignRole = "ASSIGN_ROLE";
        public const string AssignToManagers = "ASSIGN_TO_MANAGERS";
        public const string AssignOutsideScope = "ASSIGN_OUTSIDE_SCOPE";

        // Leave
        public const string ApproveLeave = "APPROVE_LEAVE";
        public const string RejectLeave = "REJECT_LEAVE";

        // Reports
        public const string ViewScopedReports = "VIEW_SCOPED_REPORTS";
        public const string ViewCompanyReports = "VIEW_COMPANY_REPORTS";

        // Notifications / escalations
        public const string ReceiveOrgEscalations = "RECEIVE_ORG_ESCALATIONS";

        // Org CRUD
        public const string CreateArea = "CREATE_AREA";
        public const string UpdateArea = "UPDATE_AREA";
        public const string DeleteArea = "DELETE_AREA";
        public const string CreateBranch = "CREATE_BRANCH";
        public const string UpdateBranch = "UPDATE_BRANCH";
        public const string DeleteBranch = "DELETE_BRANCH";
        public const string CreateDepartment = "CREATE_DEPARTMENT";
        public const string UpdateDepartment = "UPDATE_DEPARTMENT";
        public const string DeleteDepartment = "DELETE_DEPARTMENT";
        public const string CreateEmployeeType = "CREATE_EMPLOYEE_TYPE";
        public const string UpdateEmployeeType = "UPDATE_EMPLOYEE_TYPE";
        public const string DeleteEmployeeType = "DELETE_EMPLOYEE_TYPE";
        public const string CreatePermission = "CREATE_PERMISSION";

        /// <summary>Assignee / day-to-day task participation pack.</summary>
        public static readonly string[] AssigneeTaskActions =
        [
            CommentTask,
            RequestDueDateExtension,
            RequestTaskClose
        ];

        /// <summary>Manager / creator discipline &amp; review pack.</summary>
        public static readonly string[] ManagerTaskActions =
        [
            IssueWarning,
            DeleteWarning,
            IssuePenalty,
            DeletePenalty,
            ApproveTaskRequest,
            RejectTaskRequest,
            ArchiveTask
        ];

        /// <summary>
        /// Old code → new code. Used by RolePermissionPackMigrator to preserve role grants.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> LegacyCodeRenames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CLOSE_TASK_EMPLOYEE"] = RequestTaskClose,
                ["SUBMIT_DUE_DATE"] = RequestDueDateExtension,
                ["APPROVE_CLOSE_EXTEND"] = ApproveTaskRequest,
                ["REJECT_CLOSE_EXTEND"] = RejectTaskRequest,
                ["EXTEND_DUE_DATE"] = ApproveTaskRequest,
                ["SEND_WARNING"] = IssueWarning,
                ["SEND_PENALTY"] = IssuePenalty,
                ["VIEW_ALL_TASKS"] = ViewCompanyTasks,
            };
    }
}
