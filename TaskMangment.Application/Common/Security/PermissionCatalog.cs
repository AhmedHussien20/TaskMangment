namespace TaskMangment.Application.Common.Security
{
    /// <summary>
    /// Single source of truth for permission rows that must exist in DB (code + display metadata).
    /// </summary>
    public static class PermissionCatalog
    {
        public sealed record Entry(string Code, string Name, string Description);

        public static readonly IReadOnlyList<Entry> All =
        [
            // Org structure
            new(PermissionCodes.CreateArea, "Create Areas", "إمكانية إنشاء مناطق جغرافية جديدة في النظام لتنظيم الفروع والمكاتب"),
            new(PermissionCodes.UpdateArea, "Update Areas", "تعديل بيانات المناطق القائمة مثل الاسم والمسؤول والمعلومات الجغرافية"),
            new(PermissionCodes.DeleteArea, "Delete Areas", "حذف مناطق جغرافية من النظام مع التحقق من عدم وجود فروع مرتبطة بها"),
            new(PermissionCodes.CreateBranch, "Create Branches", "إنشاء فروع جديدة للشركة وتحديد موقعها والمدير المسؤول عنها"),
            new(PermissionCodes.UpdateBranch, "Update Branches", "تعديل معلومات الفروع مثل العنوان وأرقام الاتصال والإدارة"),
            new(PermissionCodes.DeleteBranch, "Delete Branches", "حذف فروع من النظام بعد التأكد من نقل الموظفين والمهام التابعة لها"),
            new(PermissionCodes.CreateDepartment, "Create Departments", "إنشاء أقسام إدارية جديدة داخل الشركة وتحديد هيكلها التنظيمي"),
            new(PermissionCodes.UpdateDepartment, "Update Departments", "تعديل بيانات الأقسام مثل الاسم والمدير والأهداف الوظيفية"),
            new(PermissionCodes.DeleteDepartment, "Delete Departments", "حذف أقسام من الهيكل التنظيمي بعد إعادة توزيع الموظفين والمهام"),
            new(PermissionCodes.CreateEmployeeType, "Create Employee Types", "إنشاء أنواع موظفين قابلة للتكوين (عمليات، محاسبة، موارد بشرية، ...)"),
            new(PermissionCodes.UpdateEmployeeType, "Update Employee Types", "تعديل أنواع الموظفين ونطاق رؤيتهم عبر الفروع"),
            new(PermissionCodes.DeleteEmployeeType, "Delete Employee Types", "حذف أنواع الموظفين غير المستخدمة"),
            new(PermissionCodes.CreatePermission, "Create Permissions", "إنشاء أذونات جديدة في النظام لتحديد صلاحيات المستخدمين"),

            // People
            new(PermissionCodes.CreateEmployee, "Create Employee Accounts", "إنشاء حسابات جديدة للموظفين وإضافة بياناتهم الشخصية والمهنية"),
            new(PermissionCodes.UpdateEmployee, "Update Employee Accounts", "تعديل بيانات الموظفين مثل المعلومات الشخصية والمهنية والمالية"),
            new(PermissionCodes.DeleteEmployee, "Delete Employee Accounts", "حذف حسابات الموظفين من النظام مع الاحتفاظ بالسجلات التاريخية"),
            new(PermissionCodes.EnableEmployee, "Enable Employee Accounts", "تفعيل حسابات الموظفين المعطلة للسماح لهم بالدخول للنظام"),
            new(PermissionCodes.DisableEmployee, "Disable Employee Accounts", "تعطيل حسابات الموظفين مؤقتاً لمنعهم من الدخول للنظام"),
            new(PermissionCodes.ViewEmployees, "View Employees", "عرض قائمة الموظفين ضمن النطاق"),
            new(PermissionCodes.AssignRole, "Assign Roles", "تعيين الأدوار للموظفين"),
            new(PermissionCodes.AssignToManagers, "Assign To Managers", "إسناد مهام إلى المديرين الأعلى تنظيمياً"),
            new(PermissionCodes.AssignOutsideScope, "Assign Outside Scope", "إسناد مهام خارج نطاق الصلاحية"),

            // Tasks — visibility
            new(PermissionCodes.ViewOwnTasks, "View Own Tasks", "عرض المهام الخاصة بالموظف"),
            new(PermissionCodes.ViewScopedTasks, "View Scoped Tasks", "عرض مهام الموظفين ضمن نطاق الصلاحية"),
            new(PermissionCodes.ViewCompanyTasks, "View Company Tasks", "عرض كل مهام الشركة"),

            // Tasks — CRUD
            new(PermissionCodes.CreateTask, "Create Tasks", "إنشاء مهام جديدة وتحديد مواعيدها وأولوياتها والمكلفين بها"),
            new(PermissionCodes.UpdateTask, "Update Tasks", "تعديل بيانات المهام القائمة مثل الوصف والأولوية والموعد النهائي"),
            new(PermissionCodes.DeleteTask, "Delete Tasks", "حذف مهام من النظام بشكل نهائي بعد انتهائها أو إلغائها"),
            new(PermissionCodes.CommentTask, "Comment on Tasks", "إضافة تعليقات وتحديثات على المهام لمتابعة التقدم والتواصل مع الفريق"),
            new(PermissionCodes.ArchiveTask, "Archive Tasks", "نقل المهام المنتهية إلى الأرشيف للحفاظ على النظام وتقليل التكدس"),

            // Tasks — assignee requests
            new(PermissionCodes.RequestTaskClose, "Request Task Close", "تقديم طلب إغلاق المهمة بعد إنجازها"),
            new(PermissionCodes.RequestDueDateExtension, "Request Due Date Extension", "تقديم طلب تمديد الموعد النهائي للمهمة"),

            // Tasks — review / discipline
            new(PermissionCodes.ApproveTaskRequest, "Approve Task Requests", "الموافقة على طلبات إغلاق المهام أو تمديد مواعيدها"),
            new(PermissionCodes.RejectTaskRequest, "Reject Task Requests", "رفض طلبات إغلاق المهام أو تمديد مواعيدها مع ذكر الأسباب"),
            new(PermissionCodes.IssueWarning, "Issue Warnings", "إصدار إنذارات رسمية للموظفين بشأن الأداء أو الالتزام"),
            new(PermissionCodes.DeleteWarning, "Delete Warnings", "حذف الإنذارات من سجلات الموظفين"),
            new(PermissionCodes.IssuePenalty, "Issue Penalties", "فرض عقوبات على الموظفين المخالفين أو ضعيفي الأداء"),
            new(PermissionCodes.DeletePenalty, "Delete Penalties", "إزالة العقوبات من سجلات الموظفين"),

            // Leave
            new(PermissionCodes.ApproveLeave, "Approve Leave", "الموافقة على طلبات الإجازة"),
            new(PermissionCodes.RejectLeave, "Reject Leave", "رفض طلبات الإجازة"),

            // Reports / escalations
            new(PermissionCodes.ViewScopedReports, "View Scoped Reports", "عرض التقارير ضمن النطاق"),
            new(PermissionCodes.ViewCompanyReports, "View Company Reports", "عرض تقارير الشركة كاملة"),
            new(PermissionCodes.ReceiveOrgEscalations, "Receive Org Escalations", "استلام تصعيدات الإدارة العليا"),
        ];

        public static readonly HashSet<string> Codes =
            new(All.Select(e => e.Code), StringComparer.OrdinalIgnoreCase);
    }
}
