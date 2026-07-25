namespace TaskMangment.Application.Common.Security
{
    /// <summary>
    /// Canonical permission codes. Roles are only bags of these codes — never gate on role name/level.
    /// </summary>
    public static class PermissionCodes
    {
        // Tasks
        public const string CreateTask = "CREATE_TASK";
        public const string UpdateTask = "UPDATE_TASK";
        public const string DeleteTask = "DELETE_TASK";
        public const string CommentTask = "COMMENT_TASK";
        public const string ViewOwnTasks = "VIEW_OWN_TASKS";
        public const string ViewScopedTasks = "VIEW_SCOPED_TASKS";
        public const string ViewCompanyTasks = "VIEW_COMPANY_TASKS";
        /// <summary>Legacy alias used by reports; treated as company-wide task visibility.</summary>
        public const string ViewAllTasks = "VIEW_ALL_TASKS";

        // People / org management
        public const string ViewEmployees = "VIEW_EMPLOYEES";
        public const string CreateEmployee = "CREATE_EMPLOYEE";
        public const string UpdateEmployee = "UPDATE_EMPLOYEE";
        public const string DeleteEmployee = "DELETE_EMPLOYEE";
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

        // Org CRUD (existing)
        public const string CreateArea = "CREATE_AREA";
        public const string UpdateArea = "UPDATE_AREA";
        public const string DeleteArea = "DELETE_AREA";
        public const string CreateBranch = "CREATE_BRANCH";
        public const string UpdateBranch = "UPDATE_BRANCH";
        public const string DeleteBranch = "DELETE_BRANCH";
        public const string CreateDepartment = "CREATE_DEPARTMENT";
        public const string UpdateDepartment = "UPDATE_DEPARTMENT";
        public const string DeleteDepartment = "DELETE_DEPARTMENT";
        public const string CreatePermission = "CREATE_PERMISSION";
        public const string UpdatePermission = "UPDATE_PERMISSION";
        public const string DeletePermission = "DELETE_PERMISSION";

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
