/* Sync Permissions catalog: restore needed codes, soft-delete junk + RolePermission FKs.
   Safe / idempotent. Only touches Permissions and RolePermission. */
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @Needed TABLE (
  Code NVARCHAR(100) PRIMARY KEY,
  Name NVARCHAR(200) NOT NULL,
  Description NVARCHAR(500) NULL
);

INSERT INTO @Needed (Code, Name, Description) VALUES
(N'EDIT_COMPANY', N'Edit Company Data', N'القدرة على تعديل بيانات الشركة الأساسية مثل الاسم والعنوان وبيانات الاتصال'),
(N'CREATE_AREA', N'Create Areas', N'إمكانية إنشاء مناطق جغرافية جديدة في النظام لتنظيم الفروع والمكاتب'),
(N'UPDATE_AREA', N'Update Areas', N'تعديل بيانات المناطق القائمة مثل الاسم والمسؤول والمعلومات الجغرافية'),
(N'DELETE_AREA', N'Delete Areas', N'حذف مناطق جغرافية من النظام مع التحقق من عدم وجود فروع مرتبطة بها'),
(N'CREATE_BRANCH', N'Create Branches', N'إنشاء فروع جديدة للشركة وتحديد موقعها والمدير المسؤول عنها'),
(N'UPDATE_BRANCH', N'Update Branches', N'تعديل معلومات الفروع مثل العنوان وأرقام الاتصال والإدارة'),
(N'DELETE_BRANCH', N'Delete Branches', N'حذف فروع من النظام بعد التأكد من نقل الموظفين والمهام التابعة لها'),
(N'CREATE_DEPARTMENT', N'Create Departments', N'إنشاء أقسام إدارية جديدة داخل الشركة وتحديد هيكلها التنظيمي'),
(N'UPDATE_DEPARTMENT', N'Update Departments', N'تعديل بيانات الأقسام مثل الاسم والمدير والأهداف الوظيفية'),
(N'DELETE_DEPARTMENT', N'Delete Departments', N'حذف أقسام من الهيكل التنظيمي بعد إعادة توزيع الموظفين والمهام'),
(N'CREATE_PERMISSION', N'Create Permissions', N'إنشاء أذونات جديدة في النظام لتحديد صلاحيات المستخدمين'),
(N'UPDATE_PERMISSION', N'Update Permissions', N'تعديل بيانات الأذونات الموجودة في النظام مثل الاسم والوصف'),
(N'DELETE_PERMISSION', N'Delete Permissions', N'حذف أذونات من النظام بعد التأكد من عدم استخدامها'),
(N'CREATE_EMPLOYEE', N'Create Employee Accounts', N'إنشاء حسابات جديدة للموظفين وإضافة بياناتهم الشخصية والمهنية'),
(N'UPDATE_EMPLOYEE', N'Update Employee Accounts', N'تعديل بيانات الموظفين مثل المعلومات الشخصية والمهنية والمالية'),
(N'DELETE_EMPLOYEE', N'Delete Employee Accounts', N'حذف حسابات الموظفين من النظام مع الاحتفاظ بالسجلات التاريخية'),
(N'ENABLE_EMPLOYEE', N'Enable Employee Accounts', N'تفعيل حسابات الموظفين المعطلة للسماح لهم بالدخول للنظام'),
(N'DISABLE_EMPLOYEE', N'Disable Employee Accounts', N'تعطيل حسابات الموظفين مؤقتاً لمنعهم من الدخول للنظام'),
(N'CREATE_JOB', N'Create Jobs', N'إنشاء وظائف جديدة في الشركة وتحديد متطلباتها ومسؤولياتها'),
(N'UPDATE_JOB', N'Update Jobs', N'تعديل بيانات الوظائف القائمة مثل المهام والمسؤوليات والمتطلبات'),
(N'DELETE_JOB', N'Delete Jobs', N'حذف وظائف من النظام بعد التأكد من عدم وجود موظفين يشغلونها'),
(N'CREATE_TASK', N'Create Tasks', N'إنشاء مهام جديدة وتحديد مواعيدها وأولوياتها والمكلفين بها'),
(N'UPDATE_TASK', N'Update Tasks', N'تعديل بيانات المهام القائمة مثل الوصف والأولوية والموعد النهائي'),
(N'DELETE_TASK', N'Delete Tasks', N'حذف مهام من النظام بشكل نهائي بعد انتهائها أو إلغائها'),
(N'COMMENT_TASK', N'Comment on Tasks', N'إضافة تعليقات وتحديثات على المهام لمتابعة التقدم والتواصل مع الفريق'),
(N'VIEW_OWN_TASKS', N'View Own Tasks', N'عرض المهام الخاصة بالموظف'),
(N'VIEW_SCOPED_TASKS', N'View Scoped Tasks', N'عرض مهام الموظفين ضمن نطاق الصلاحية'),
(N'VIEW_COMPANY_TASKS', N'View Company Tasks', N'عرض كل مهام الشركة'),
(N'VIEW_ALL_TASKS', N'View All Tasks', N'عرض كل المهام (توافق قديم)'),
(N'EXTEND_DUE_DATE', N'Extend Due Date', N'تمديد الموعد النهائي للمهام بناءً على الطلبات أو الظروف الطارئة'),
(N'SUBMIT_DUE_DATE', N'Submit Due Date', N'تقديم طلبات تمديد المواعيد النهائية للمهام للإدارة للموافقة'),
(N'APPROVE_CLOSE_EXTEND', N'Approve Task Close/Extend Requests', N'الموافقة على طلبات إغلاق المهام أو تمديد مواعيدها النهائية'),
(N'REJECT_CLOSE_EXTEND', N'Reject Task Close/Extend Requests', N'رفض طلبات إغلاق المهام أو تمديد مواعيدها النهائية مع ذكر الأسباب'),
(N'CLOSE_TASK_EMPLOYEE', N'Close Task for Employee', N'إغلاق المهام المكلف بها الموظف بعد إكمالها أو حسب التوجيهات'),
(N'ARCHIVE_TASK', N'Archive Tasks', N'نقل المهام المنتهية إلى الأرشيف للحفاظ على النظام وتقليل التكدس'),
(N'TRANSFER_TASK', N'Transfer Task to Employee', N'نقل المهام من موظف لآخر بناءً على الكفاءة أو الأحمال الوظيفية'),
(N'EXEMPT_EMPLOYEE_TASK', N'Exempt Employee from Task', N'إعفاء الموظف من مهمة معينة لأسباب طارئة أو تنظيمية'),
(N'REOPEN_TASK_EMPLOYEE', N'Reopen Task for Employee', N'إعادة فتح المهام المغلقة لإجراء تعديلات أو تكملة متطلبات إضافية'),
(N'SEND_WARNING', N'Send Warnings', N'إرسال إنذارات رسمية للموظفين بشأن الأداء أو الالتزام باللوائح'),
(N'DELETE_WARNING', N'Delete Warnings', N'حذف الإنذارات من سجلات الموظفين بعد معالجة القضايا أو تصحيح الأخطاء'),
(N'SEND_PENALTY', N'Send Penalties', N'فرض عقوبات على الموظفين المخالفين للوائح أو ضعيفي الأداء'),
(N'DELETE_PENALTY', N'Delete Penalties', N'إزالة العقوبات من سجلات الموظفين بعد تصحيح المخالفات أو انتهاء المدة'),
(N'VIEW_EMPLOYEES', N'View Employees', N'عرض قائمة الموظفين ضمن النطاق'),
(N'ASSIGN_ROLE', N'Assign Roles', N'تعيين الأدوار للموظفين'),
(N'MANAGE_MANAGER_SCOPE', N'Manage Manager Scope', N'إدارة فروع وأنواع الموظفين ضمن نطاق المدير'),
(N'ASSIGN_TO_MANAGERS', N'Assign To Managers', N'إسناد مهام إلى المديرين الأعلى تنظيمياً'),
(N'ASSIGN_OUTSIDE_SCOPE', N'Assign Outside Scope', N'إسناد مهام خارج نطاق الصلاحية'),
(N'APPROVE_LEAVE', N'Approve Leave', N'الموافقة على طلبات الإجازة'),
(N'REJECT_LEAVE', N'Reject Leave', N'رفض طلبات الإجازة'),
(N'VIEW_SCOPED_REPORTS', N'View Scoped Reports', N'عرض التقارير ضمن النطاق'),
(N'VIEW_COMPANY_REPORTS', N'View Company Reports', N'عرض تقارير الشركة كاملة'),
(N'RECEIVE_ORG_ESCALATIONS', N'Receive Org Escalations', N'استلام تصعيدات الإدارة العليا');

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @Keepers TABLE (Code NVARCHAR(100) PRIMARY KEY, KeeperId INT NOT NULL);

-- Pick keeper = active min Id, else overall min Id
INSERT INTO @Keepers (Code, KeeperId)
SELECT n.Code,
       COALESCE(
         (SELECT MIN(p.Id) FROM Permissions p WHERE p.Code = n.Code AND p.IsDeleted = 0),
         (SELECT MIN(p.Id) FROM Permissions p WHERE p.Code = n.Code)
       )
FROM @Needed n;

-- Restore keepers that are soft-deleted
UPDATE p
SET p.IsDeleted = 0,
    p.DeletedDate = NULL,
    p.ModifiedDate = @Now,
    p.Name = n.Name,
    p.Description = n.Description
FROM Permissions p
INNER JOIN @Keepers k ON k.KeeperId = p.Id
INNER JOIN @Needed n ON n.Code = k.Code
WHERE p.IsDeleted = 1 OR p.Name <> n.Name OR ISNULL(p.Description, N'') <> ISNULL(n.Description, N'');

-- Insert missing codes
INSERT INTO Permissions (Code, Name, Description, CreatedDate, IsDeleted)
SELECT n.Code, n.Name, n.Description, @Now, 0
FROM @Needed n
WHERE NOT EXISTS (SELECT 1 FROM Permissions p WHERE p.Code = n.Code);

-- Refresh keepers after inserts
DELETE FROM @Keepers;
INSERT INTO @Keepers (Code, KeeperId)
SELECT n.Code, MIN(p.Id)
FROM @Needed n
INNER JOIN Permissions p ON p.Code = n.Code
GROUP BY n.Code;

-- Soft-delete duplicate rows for catalog codes
UPDATE p
SET p.IsDeleted = 1,
    p.DeletedDate = @Now
FROM Permissions p
INNER JOIN @Keepers k ON k.Code = p.Code
WHERE p.Id <> k.KeeperId AND p.IsDeleted = 0;

-- Remap RolePermission from duplicate permission ids onto keeper
;WITH DupLinks AS (
  SELECT rp.Id AS RolePermissionId, rp.RoleId, rp.PermissionId, rp.IsAssigned, rp.IsDeleted,
         k.KeeperId
  FROM RolePermission rp
  INNER JOIN Permissions p ON p.Id = rp.PermissionId
  INNER JOIN @Keepers k ON k.Code = p.Code
  WHERE rp.PermissionId <> k.KeeperId
)
UPDATE rp
SET rp.PermissionId = d.KeeperId,
    rp.IsDeleted = CASE WHEN rp.IsAssigned = 1 THEN 0 ELSE rp.IsDeleted END,
    rp.DeletedDate = CASE WHEN rp.IsAssigned = 1 THEN NULL ELSE rp.DeletedDate END,
    rp.ModifiedDate = @Now
FROM RolePermission rp
INNER JOIN DupLinks d ON d.RolePermissionId = rp.Id
WHERE NOT EXISTS (
  SELECT 1 FROM RolePermission x
  WHERE x.RoleId = d.RoleId AND x.PermissionId = d.KeeperId AND x.Id <> rp.Id
);

-- Retire remaining dup links when keeper link already exists
UPDATE rp
SET rp.IsDeleted = 1,
    rp.IsAssigned = 0,
    rp.DeletedDate = @Now
FROM RolePermission rp
INNER JOIN Permissions p ON p.Id = rp.PermissionId
INNER JOIN @Keepers k ON k.Code = p.Code
WHERE rp.PermissionId <> k.KeeperId
  AND rp.IsDeleted = 0;

-- Soft-delete unknown (junk) permissions + their RolePermission FKs
DECLARE @Junk TABLE (Id INT PRIMARY KEY);
INSERT INTO @Junk (Id)
SELECT p.Id
FROM Permissions p
WHERE p.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM @Needed n WHERE n.Code = p.Code);

UPDATE rp
SET rp.IsDeleted = 1,
    rp.IsAssigned = 0,
    rp.DeletedDate = @Now
FROM RolePermission rp
INNER JOIN @Junk j ON j.Id = rp.PermissionId
WHERE rp.IsDeleted = 0;

UPDATE p
SET p.IsDeleted = 1,
    p.DeletedDate = @Now
FROM Permissions p
INNER JOIN @Junk j ON j.Id = p.Id;

-- Report
SELECT 'ActiveCatalogCount' AS Metric, COUNT(*) AS Value
FROM Permissions p
INNER JOIN @Needed n ON n.Code = p.Code
WHERE p.IsDeleted = 0;

SELECT 'MissingActive' AS Metric, n.Code AS Value
FROM @Needed n
WHERE NOT EXISTS (SELECT 1 FROM Permissions p WHERE p.Code = n.Code AND p.IsDeleted = 0);

SELECT 'ActiveJunkLeft' AS Metric, p.Code AS Value
FROM Permissions p
WHERE p.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM @Needed n WHERE n.Code = p.Code);

COMMIT TRAN;
