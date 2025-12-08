using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Seeding
{
    public class DataSeeder
    {
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<RolePermission> _rolePermRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            IRepository<Permission> permissionRepo,
            IRepository<Role> roleRepo,
            IRepository<RolePermission> rolePermRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            ILogger<DataSeeder> logger)
        {
            _permissionRepo = permissionRepo;
            _roleRepo = roleRepo;
            _rolePermRepo = rolePermRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding process...");

                await SeedPermissionsAsync();
                await SeedRolesAsync();
                await AssignPermissionsToRolesAsync();

                _logger.LogInformation("Data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during data seeding");
                throw; 
            }
        }

        private async Task SeedPermissionsAsync()
        {
            //if (!await _permissionrepo.getall().anyasync())
            //{
                _logger.LogInformation("Seeding permissions...");

            var permissions = new List<Permission>
{
    new Permission
    {
        Code = "EDIT_COMPANY",
        Name = "Edit Company Data",
        Description = "القدرة على تعديل بيانات الشركة الأساسية مثل الاسم والعنوان وبيانات الاتصال",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CREATE_AREA",
        Name = "Create Areas",
        Description = "إمكانية إنشاء مناطق جغرافية جديدة في النظام لتنظيم الفروع والمكاتب",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "UPDATE_AREA",
        Name = "Update Areas",
        Description = "تعديل بيانات المناطق القائمة مثل الاسم والمسؤول والمعلومات الجغرافية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_AREA",
        Name = "Delete Areas",
        Description = "حذف مناطق جغرافية من النظام مع التحقق من عدم وجود فروع مرتبطة بها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CREATE_BRANCH",
        Name = "Create Branches",
        Description = "إنشاء فروع جديدة للشركة وتحديد موقعها والمدير المسؤول عنها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "UPDATE_BRANCH",
        Name = "Update Branches",
        Description = "تعديل معلومات الفروع مثل العنوان وأرقام الاتصال والإدارة",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_BRANCH",
        Name = "Delete Branches",
        Description = "حذف فروع من النظام بعد التأكد من نقل الموظفين والمهام التابعة لها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CREATE_DEPARTMENT",
        Name = "Create Departments",
        Description = "إنشاء أقسام إدارية جديدة داخل الشركة وتحديد هيكلها التنظيمي",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "UPDATE_DEPARTMENT",
        Name = "Update Departments",
        Description = "تعديل بيانات الأقسام مثل الاسم والمدير والأهداف الوظيفية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_DEPARTMENT",
        Name = "Delete Departments",
        Description = "حذف أقسام من الهيكل التنظيمي بعد إعادة توزيع الموظفين والمهام",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CREATE_PERMISSION",
        Name = "Create Permissions",
        Description = "إنشاء أذونات جديدة في النظام لتحديد صلاحيات المستخدمين",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "UPDATE_PERMISSION",
        Name = "Update Permissions",
        Description = "تعديل بيانات الأذونات الموجودة في النظام مثل الاسم والوصف",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_PERMISSION",
        Name = "Delete Permissions",
        Description = "حذف أذونات من النظام بعد التأكد من عدم استخدامها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CREATE_EMPLOYEE",
        Name = "Create Employee Accounts",
        Description = "إنشاء حسابات جديدة للموظفين وإضافة بياناتهم الشخصية والمهنية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "UPDATE_EMPLOYEE",
        Name = "Update Employee Accounts",
        Description = "تعديل بيانات الموظفين مثل المعلومات الشخصية والمهنية والمالية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_EMPLOYEE",
        Name = "Delete Employee Accounts",
        Description = "حذف حسابات الموظفين من النظام مع الاحتفاظ بالسجلات التاريخية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "ENABLE_EMPLOYEE",
        Name = "Enable Employee Accounts",
        Description = "تفعيل حسابات الموظفين المعطلة للسماح لهم بالدخول للنظام",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DISABLE_EMPLOYEE",
        Name = "Disable Employee Accounts",
        Description = "تعطيل حسابات الموظفين مؤقتاً لمنعهم من الدخول للنظام",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CREATE_JOB",
        Name = "Create Jobs",
        Description = "إنشاء وظائف جديدة في الشركة وتحديد متطلباتها ومسؤولياتها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "UPDATE_JOB",
        Name = "Update Jobs",
        Description = "تعديل بيانات الوظائف القائمة مثل المهام والمسؤوليات والمتطلبات",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_JOB",
        Name = "Delete Jobs",
        Description = "حذف وظائف من النظام بعد التأكد من عدم وجود موظفين يشغلونها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CREATE_TASK",
        Name = "Create Tasks",
        Description = "إنشاء مهام جديدة وتحديد مواعيدها وأولوياتها والمكلفين بها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "UPDATE_TASK",
        Name = "Update Tasks",
        Description = "تعديل بيانات المهام القائمة مثل الوصف والأولوية والموعد النهائي",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_TASK",
        Name = "Delete Tasks",
        Description = "حذف مهام من النظام بشكل نهائي بعد انتهائها أو إلغائها",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "COMMENT_TASK",
        Name = "Comment on Tasks",
        Description = "إضافة تعليقات وتحديثات على المهام لمتابعة التقدم والتواصل مع الفريق",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "EXTEND_DUE_DATE",
        Name = "Extend Due Date",
        Description = "تمديد الموعد النهائي للمهام بناءً على الطلبات أو الظروف الطارئة",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "SUBMIT_DUE_DATE",
        Name = "Submit Due Date",
        Description = "تقديم طلبات تمديد المواعيد النهائية للمهام للإدارة للموافقة",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "APPROVE_CLOSE_EXTEND",
        Name = "Approve Task Close/Extend Requests",
        Description = "الموافقة على طلبات إغلاق المهام أو تمديد مواعيدها النهائية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "REJECT_CLOSE_EXTEND",
        Name = "Reject Task Close/Extend Requests",
        Description = "رفض طلبات إغلاق المهام أو تمديد مواعيدها النهائية مع ذكر الأسباب",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "CLOSE_TASK_EMPLOYEE",
        Name = "Close Task for Employee",
        Description = "إغلاق المهام المكلف بها الموظف بعد إكمالها أو حسب التوجيهات",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "ARCHIVE_TASK",
        Name = "Archive Tasks",
        Description = "نقل المهام المنتهية إلى الأرشيف للحفاظ على النظام وتقليل التكدس",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "TRANSFER_TASK",
        Name = "Transfer Task to Employee",
        Description = "نقل المهام من موظف لآخر بناءً على الكفاءة أو الأحمال الوظيفية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "EXEMPT_EMPLOYEE_TASK",
        Name = "Exempt Employee from Task",
        Description = "إعفاء الموظف من مهمة معينة لأسباب طارئة أو تنظيمية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "REOPEN_TASK_EMPLOYEE",
        Name = "Reopen Task for Employee",
        Description = "إعادة فتح المهام المغلقة لإجراء تعديلات أو تكملة متطلبات إضافية",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "SEND_WARNING",
        Name = "Send Warnings",
        Description = "إرسال إنذارات رسمية للموظفين بشأن الأداء أو الالتزام باللوائح",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_WARNING",
        Name = "Delete Warnings",
        Description = "حذف الإنذارات من سجلات الموظفين بعد معالجة القضايا أو تصحيح الأخطاء",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "SEND_PENALTY",
        Name = "Send Penalties",
        Description = "فرض عقوبات على الموظفين المخالفين للوائح أو ضعيفي الأداء",
        CreatedDate = DateTime.UtcNow
    },

    new Permission
    {
        Code = "DELETE_PENALTY",
        Name = "Delete Penalties",
        Description = "إزالة العقوبات من سجلات الموظفين بعد تصحيح المخالفات أو انتهاء المدة",
        CreatedDate = DateTime.UtcNow
    }
};
            // إضافة جميع الصلاحيات دفعة واحدة
            await _permissionRepo.AddRangeAsync(permissions);
                await _permissionRepo.SaveChangesAsync();

                _logger.LogInformation($"Seeded {permissions.Count} permissions.");
            //}
            //else
            //{
            //    _logger.LogInformation("Permissions already exist. Skipping...");
            //}
        }

        private async Task SeedRolesAsync()
        {
            //if (!await _roleRepo.GetAll().AnyAsync())
            //{
                _logger.LogInformation("Seeding roles...");

                var roles = new List<Role>
                {
                     new Role
                    {
                        Name = "Manager",
                        CompanyId = 1,
                        Description = "Manager company responsible for whole things",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Technical Support",
                        CompanyId = 1, 
                        Description = "Handles technical issues and support requests",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "CEO",
                        CompanyId = 1,
                        Description = "Chief Executive Officer with all permissions",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Branch Manager",
                        CompanyId = 1,
                        Description = "Manages branch operations and employees",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Group 10",
                        CompanyId = 1,
                        Description = "Special group with specific permissions",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "UV",
                        CompanyId = 1,
                        Description = "UV group role",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                await _roleRepo.AddRangeAsync(roles);
                await _roleRepo.SaveChangesAsync();

                _logger.LogInformation($"Seeded {roles.Count} roles.");
            //}
            //else
            //{
            //    _logger.LogInformation("Roles already exist. Skipping...");
            //}
        }

        private async Task AssignPermissionsToRolesAsync()
        {
            _logger.LogInformation("Assigning permissions to roles...");

            var fullAccessRoles = await _roleRepo
                .GetAll(r => r.Name == "CEO" || r.Name == "Manager")
                .ToListAsync();

            var allPermissions = await _permissionRepo.GetAll().ToListAsync();

            foreach (var role in fullAccessRoles)
            {
                var existingPermissions = await _rolePermRepo
                    .GetAll(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                var newPermissions = allPermissions
                    .Where(p => !existingPermissions.Contains(p.Id))
                    .Select(p => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = p.Id,
                        CreatedDate = DateTime.UtcNow
                    });

                if (newPermissions.Any())
                {
                    await _rolePermRepo.AddRangeAsync(newPermissions);
                    await _rolePermRepo.SaveChangesAsync();
                    _logger.LogInformation($"Assigned {newPermissions.Count()} permissions to {role.Name} role.");
                }
            }

            _logger.LogInformation("Permission assignment completed for full access roles.");
        }
    }
}
