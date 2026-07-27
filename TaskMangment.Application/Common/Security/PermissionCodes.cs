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
        /// <summary>Legacy alias used by reports; treated as company-wide task visibility.</summary>
        public const string ViewAllTasks = "VIEW_ALL_TASKS";

        // Tasks — workflow
        public const string ExtendDueDate = "EXTEND_DUE_DATE";
        public const string SubmitDueDate = "SUBMIT_DUE_DATE";
        public const string ApproveCloseExtend = "APPROVE_CLOSE_EXTEND";
        public const string RejectCloseExtend = "REJECT_CLOSE_EXTEND";
        public const string CloseTaskEmployee = "CLOSE_TASK_EMPLOYEE";
        public const string ArchiveTask = "ARCHIVE_TASK";
        public const string TransferTask = "TRANSFER_TASK";
        public const string ExemptEmployeeTask = "EXEMPT_EMPLOYEE_TASK";
        public const string ReopenTaskEmployee = "REOPEN_TASK_EMPLOYEE";
        public const string SendWarning = "SEND_WARNING";
        public const string DeleteWarning = "DELETE_WARNING";
        public const string SendPenalty = "SEND_PENALTY";
        public const string DeletePenalty = "DELETE_PENALTY";

        // People / org management
        public const string ViewEmployees = "VIEW_EMPLOYEES";
        public const string CreateEmployee = "CREATE_EMPLOYEE";
        public const string UpdateEmployee = "UPDATE_EMPLOYEE";
        public const string DeleteEmployee = "DELETE_EMPLOYEE";
        public const string EnableEmployee = "ENABLE_EMPLOYEE";
        public const string DisableEmployee = "DISABLE_EMPLOYEE";
        public const string AssignRole = "ASSIGN_ROLE";
        public const string ManageManagerScope = "MANAGE_MANAGER_SCOPE";
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
        public const string EditCompany = "EDIT_COMPANY";
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
        public const string UpdatePermission = "UPDATE_PERMISSION";
        public const string DeletePermission = "DELETE_PERMISSION";
        public const string CreateJob = "CREATE_JOB";
        public const string UpdateJob = "UPDATE_JOB";
        public const string DeleteJob = "DELETE_JOB";

        /// <summary>Assignee / day-to-day task participation pack.</summary>
        public static readonly string[] AssigneeTaskActions =
        [
            CommentTask,
            SubmitDueDate,
            CloseTaskEmployee
        ];

        /// <summary>Manager / creator discipline & review pack.</summary>
        public static readonly string[] ManagerTaskActions =
        [
            SendWarning,
            DeleteWarning,
            SendPenalty,
            DeletePenalty,
            ApproveCloseExtend,
            RejectCloseExtend,
            ExtendDueDate,
            ArchiveTask,
            TransferTask,
            ExemptEmployeeTask,
            ReopenTaskEmployee
        ];

        public static readonly string[] NewCatalogCodes =
        [
            ViewOwnTasks,
            ViewScopedTasks,
            ViewCompanyTasks,
            ViewAllTasks,
            ViewEmployees,
            AssignRole,
            ManageManagerScope,
            AssignToManagers,
            AssignOutsideScope,
            ApproveLeave,
            RejectLeave,
            ViewScopedReports,
            ViewCompanyReports,
            ReceiveOrgEscalations
        ];
    }
}
