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
            new(PermissionCodes.EditCompany, "Edit Company Data", "القدرة على تعديل بيانات الشركة الأساسية مثل الاسم والعنوان وبيانات الاتصال"),
            new(PermissionCodes.CreateArea, "Create Areas", "إمكانية إنشاء مناطق جغرافية جديدة في النظام لتنظيم الفروع والمكاتب"),
            new(PermissionCodes.UpdateArea, "Update Areas", "تعديل بيانات المناطق القائمة مثل الاسم والمسؤول والمعلومات الجغرافية"),
            new(PermissionCodes.DeleteArea, "Delete Areas", "حذف مناطق جغرافية من النظام مع التحقق من عدم وجود فروع مرتبطة بها"),
            new(PermissionCodes.CreateBranch, "Create Branches", "إنشاء فروع جديدة للشركة وتحديد موقعها والمدير المسؤول عنها"),
            new(PermissionCodes.UpdateBranch, "Update Branches", "تعديل معلومات الفروع مثل العنوان وأرقام الاتصال والإدارة"),
            new(PermissionCodes.DeleteBranch, "Delete Branches", "حذف فروع من النظام بعد التأكد من نقل الموظفين والمهام التابعة لها"),
            new(PermissionCodes.CreateDepartment, "Create Departments", "إنشاء أقسام إدارية جديدة داخل الشركة وتحديد هيكلها التنظيمي"),
            new(PermissionCodes.UpdateDepartment, "Update Departments", "تعديل بيانات الأقسام مثل الاسم والمدير والأهداف الوظيفية"),
            new(PermissionCodes.DeleteDepartment, "Delete Departments", "حذف أقسام من الهيكل التنظيمي بعد إعادة توزيع الموظفين والمهام"),
            new(PermissionCodes.CreatePermission, "Create Permissions", "إنشاء أذونات جديدة في النظام لتحديد صلاحيات المستخدمين"),
            new(PermissionCodes.UpdatePermission, "Update Permissions", "تعديل بيانات الأذونات الموجودة في النظام مثل الاسم والوصف"),
            new(PermissionCodes.DeletePermission, "Delete Permissions", "حذف أذونات من النظام بعد التأكد من عدم استخدامها"),
            new(PermissionCodes.CreateEmployee, "Create Employee Accounts", "إنشاء حسابات جديدة للموظفين وإضافة بياناتهم الشخصية والمهنية"),
            new(PermissionCodes.UpdateEmployee, "Update Employee Accounts", "تعديل بيانات الموظفين مثل المعلومات الشخصية والمهنية والمالية"),
            new(PermissionCodes.DeleteEmployee, "Delete Employee Accounts", "حذف حسابات الموظفين من النظام مع الاحتفاظ بالسجلات التاريخية"),
            new(PermissionCodes.EnableEmployee, "Enable Employee Accounts", "تفعيل حسابات الموظفين المعطلة للسماح لهم بالدخول للنظام"),
            new(PermissionCodes.DisableEmployee, "Disable Employee Accounts", "تعطيل حسابات الموظفين مؤقتاً لمنعهم من الدخول للنظام"),
            new(PermissionCodes.CreateJob, "Create Jobs", "إنشاء وظائف جديدة في الشركة وتحديد متطلباتها ومسؤولياتها"),
            new(PermissionCodes.UpdateJob, "Update Jobs", "تعديل بيانات الوظائف القائمة مثل المهام والمسؤوليات والمتطلبات"),
            new(PermissionCodes.DeleteJob, "Delete Jobs", "حذف وظائف من النظام بعد التأكد من عدم وجود موظفين يشغلونها"),
            new(PermissionCodes.CreateTask, "Create Tasks", "إنشاء مهام جديدة وتحديد مواعيدها وأولوياتها والمكلفين بها"),
            new(PermissionCodes.UpdateTask, "Update Tasks", "تعديل بيانات المهام القائمة مثل الوصف والأولوية والموعد النهائي"),
            new(PermissionCodes.DeleteTask, "Delete Tasks", "حذف مهام من النظام بشكل نهائي بعد انتهائها أو إلغائها"),
            new(PermissionCodes.CommentTask, "Comment on Tasks", "إضافة تعليقات وتحديثات على المهام لمتابعة التقدم والتواصل مع الفريق"),
            new(PermissionCodes.ViewOwnTasks, "View Own Tasks", "عرض المهام الخاصة بالموظف"),
            new(PermissionCodes.ViewScopedTasks, "View Scoped Tasks", "عرض مهام الموظفين ضمن نطاق الصلاحية"),
            new(PermissionCodes.ViewCompanyTasks, "View Company Tasks", "عرض كل مهام الشركة"),
            new(PermissionCodes.ViewAllTasks, "View All Tasks", "عرض كل المهام (توافق قديم)"),
            new(PermissionCodes.ExtendDueDate, "Extend Due Date", "تمديد الموعد النهائي للمهام بناءً على الطلبات أو الظروف الطارئة"),
            new(PermissionCodes.SubmitDueDate, "Submit Due Date", "تقديم طلبات تمديد المواعيد النهائية للمهام للإدارة للموافقة"),
            new(PermissionCodes.ApproveCloseExtend, "Approve Task Close/Extend Requests", "الموافقة على طلبات إغلاق المهام أو تمديد مواعيدها النهائية"),
            new(PermissionCodes.RejectCloseExtend, "Reject Task Close/Extend Requests", "رفض طلبات إغلاق المهام أو تمديد مواعيدها النهائية مع ذكر الأسباب"),
            new(PermissionCodes.CloseTaskEmployee, "Close Task for Employee", "إغلاق المهام المكلف بها الموظف بعد إكمالها أو حسب التوجيهات"),
            new(PermissionCodes.ArchiveTask, "Archive Tasks", "نقل المهام المنتهية إلى الأرشيف للحفاظ على النظام وتقليل التكدس"),
            new(PermissionCodes.TransferTask, "Transfer Task to Employee", "نقل المهام من موظف لآخر بناءً على الكفاءة أو الأحمال الوظيفية"),
            new(PermissionCodes.ExemptEmployeeTask, "Exempt Employee from Task", "إعفاء الموظف من مهمة معينة لأسباب طارئة أو تنظيمية"),
            new(PermissionCodes.ReopenTaskEmployee, "Reopen Task for Employee", "إعادة فتح المهام المغلقة لإجراء تعديلات أو تكملة متطلبات إضافية"),
            new(PermissionCodes.SendWarning, "Send Warnings", "إرسال إنذارات رسمية للموظفين بشأن الأداء أو الالتزام باللوائح"),
            new(PermissionCodes.DeleteWarning, "Delete Warnings", "حذف الإنذارات من سجلات الموظفين بعد معالجة القضايا أو تصحيح الأخطاء"),
            new(PermissionCodes.SendPenalty, "Send Penalties", "فرض عقوبات على الموظفين المخالفين للوائح أو ضعيفي الأداء"),
            new(PermissionCodes.DeletePenalty, "Delete Penalties", "إزالة العقوبات من سجلات الموظفين بعد تصحيح المخالفات أو انتهاء المدة"),
            new(PermissionCodes.ViewEmployees, "View Employees", "عرض قائمة الموظفين ضمن النطاق"),
            new(PermissionCodes.AssignRole, "Assign Roles", "تعيين الأدوار للموظفين"),
            new(PermissionCodes.ManageManagerScope, "Manage Manager Scope", "إدارة فروع وأنواع الموظفين ضمن نطاق المدير"),
            new(PermissionCodes.AssignToManagers, "Assign To Managers", "إسناد مهام إلى المديرين الأعلى تنظيمياً"),
            new(PermissionCodes.AssignOutsideScope, "Assign Outside Scope", "إسناد مهام خارج نطاق الصلاحية"),
            new(PermissionCodes.ApproveLeave, "Approve Leave", "الموافقة على طلبات الإجازة"),
            new(PermissionCodes.RejectLeave, "Reject Leave", "رفض طلبات الإجازة"),
            new(PermissionCodes.ViewScopedReports, "View Scoped Reports", "عرض التقارير ضمن النطاق"),
            new(PermissionCodes.ViewCompanyReports, "View Company Reports", "عرض تقارير الشركة كاملة"),
            new(PermissionCodes.ReceiveOrgEscalations, "Receive Org Escalations", "استلام تصعيدات الإدارة العليا"),
        ];

        public static readonly HashSet<string> Codes =
            new(All.Select(e => e.Code), StringComparer.OrdinalIgnoreCase);
    }
}
