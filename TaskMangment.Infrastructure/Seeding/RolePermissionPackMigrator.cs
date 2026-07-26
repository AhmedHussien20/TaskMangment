using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    /// <summary>
    /// Idempotent: ensures new permission codes exist and grants level-equivalent packs to existing roles.
    /// Safe to run on every startup during the RoleLevel → permission cutover.
    /// </summary>
    public class RolePermissionPackMigrator
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RolePermissionPackMigrator> _logger;

        private static readonly Dictionary<string, (string Name, string Description)> Catalog = new()
        {
            [PermissionCodes.ViewOwnTasks] = ("View Own Tasks", "عرض المهام الخاصة بالموظف"),
            [PermissionCodes.ViewScopedTasks] = ("View Scoped Tasks", "عرض مهام الموظفين ضمن نطاق الصلاحية"),
            [PermissionCodes.ViewCompanyTasks] = ("View Company Tasks", "عرض كل مهام الشركة"),
            [PermissionCodes.ViewAllTasks] = ("View All Tasks", "عرض كل المهام (توافق قديم)"),
            [PermissionCodes.ViewEmployees] = ("View Employees", "عرض قائمة الموظفين ضمن النطاق"),
            [PermissionCodes.AssignRole] = ("Assign Roles", "تعيين الأدوار للموظفين"),
            [PermissionCodes.ManageManagerScope] = ("Manage Manager Scope", "إدارة فروع وأنواع الموظفين ضمن نطاق المدير"),
            [PermissionCodes.AssignToManagers] = ("Assign To Managers", "إسناد مهام إلى المديرين الأعلى تنظيمياً"),
            [PermissionCodes.AssignOutsideScope] = ("Assign Outside Scope", "إسناد مهام خارج نطاق الصلاحية"),
            [PermissionCodes.ApproveLeave] = ("Approve Leave", "الموافقة على طلبات الإجازة"),
            [PermissionCodes.RejectLeave] = ("Reject Leave", "رفض طلبات الإجازة"),
            [PermissionCodes.ViewScopedReports] = ("View Scoped Reports", "عرض التقارير ضمن النطاق"),
            [PermissionCodes.ViewCompanyReports] = ("View Company Reports", "عرض تقارير الشركة كاملة"),
            [PermissionCodes.ReceiveOrgEscalations] = ("Receive Org Escalations", "استلام تصعيدات الإدارة العليا"),
        };

        public RolePermissionPackMigrator(AppDbContext db, ILogger<RolePermissionPackMigrator> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task MigrateAsync()
        {
            await DeduplicatePermissionCodesAsync();
            await EnsureCatalogAsync();
            await GrantPacksByRoleLevelAsync();
            await _db.SaveChangesAsync();
            _logger.LogInformation("Role permission pack migration completed.");
        }

        /// <summary>
        /// Soft-deletes duplicate Permission rows with the same Code (keeps lowest Id).
        /// Older DataSeeder runs could insert the same code more than once.
        /// </summary>
        private async Task DeduplicatePermissionCodesAsync()
        {
            var rows = await _db.Permissions
                .Where(p => !p.IsDeleted)
                .Select(p => new { p.Id, p.Code })
                .ToListAsync();

            var duplicateIds = rows
                .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(x => x.Id).Skip(1).Select(x => x.Id))
                .ToList();

            if (duplicateIds.Count == 0)
                return;

            var now = DateTime.UtcNow;
            var duplicates = await _db.Permissions
                .Where(p => duplicateIds.Contains(p.Id))
                .ToListAsync();

            foreach (var dup in duplicates)
            {
                dup.IsDeleted = true;
                dup.DeletedDate = now;
            }

            // Drop RolePermission links that pointed at soft-deleted duplicates
            var orphanLinks = await _db.RolePermissions
                .Where(rp => duplicateIds.Contains(rp.PermissionId) && !rp.IsDeleted)
                .ToListAsync();

            foreach (var link in orphanLinks)
            {
                link.IsDeleted = true;
                link.DeletedDate = now;
                link.IsAssigned = false;
            }

            _logger.LogWarning(
                "Soft-deleted {Count} duplicate Permission rows (and {LinkCount} role links).",
                duplicates.Count,
                orphanLinks.Count);

            await _db.SaveChangesAsync();
        }

        private async Task EnsureCatalogAsync()
        {
            var existing = await _db.Permissions
                .Where(p => !p.IsDeleted)
                .Select(p => p.Code)
                .ToListAsync();

            var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            foreach (var (code, meta) in Catalog)
            {
                if (existingSet.Contains(code))
                    continue;

                _db.Permissions.Add(new Permission
                {
                    Code = code,
                    Name = meta.Name,
                    Description = meta.Description,
                    CreatedDate = now
                });
            }
        }

        private async Task GrantPacksByRoleLevelAsync()
        {
            await _db.SaveChangesAsync();

            // DB may contain duplicate Codes from older seed runs — keep lowest Id per code.
            var permissions = (await _db.Permissions
                    .Where(p => !p.IsDeleted)
                    .Select(p => new { p.Code, p.Id })
                    .ToListAsync())
                .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Min(x => x.Id), StringComparer.OrdinalIgnoreCase);

            var roles = await _db.Roles
                .Where(r => !r.IsDeleted)
                .Include(r => r.RolePermissions.Where(rp => !rp.IsDeleted))
                .ToListAsync();

            foreach (var role in roles)
            {
                var pack = GetPackForLevel(role.Level);
                if (pack.Count == 0)
                    continue;

                var assignedIds = role.RolePermissions
                    .Where(rp => rp.IsAssigned)
                    .Select(rp => rp.PermissionId)
                    .ToHashSet();

                foreach (var code in pack)
                {
                    if (!permissions.TryGetValue(code, out var permissionId))
                        continue;

                    var existing = role.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
                    if (existing != null)
                    {
                        if (!existing.IsAssigned || existing.IsDeleted)
                        {
                            existing.IsAssigned = true;
                            existing.IsDeleted = false;
                            existing.ModifiedDate = DateTime.UtcNow;
                        }
                        continue;
                    }

                    if (assignedIds.Contains(permissionId))
                        continue;

                    role.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        IsAssigned = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }
        }

        /// <summary>
        /// Maps legacy Role.Level to permission packs so behavior stays equivalent after cutover.
        /// </summary>
        public static IReadOnlyList<string> GetPackForLevel(int level)
        {
            if (level >= (int)RoleLevelEnum.Admin)
            {
                return
                [
                    PermissionCodes.ViewOwnTasks,
                    PermissionCodes.ViewScopedTasks,
                    PermissionCodes.ViewCompanyTasks,
                    PermissionCodes.ViewAllTasks,
                    PermissionCodes.ViewEmployees,
                    PermissionCodes.AssignRole,
                    PermissionCodes.ManageManagerScope,
                    PermissionCodes.AssignToManagers,
                    PermissionCodes.ApproveLeave,
                    PermissionCodes.RejectLeave,
                    PermissionCodes.ViewScopedReports,
                    PermissionCodes.ViewCompanyReports,
                    PermissionCodes.ReceiveOrgEscalations,
                    PermissionCodes.CreateArea,
                    PermissionCodes.UpdateArea,
                    PermissionCodes.DeleteArea,
                    PermissionCodes.CreateBranch,
                    PermissionCodes.UpdateBranch,
                    PermissionCodes.DeleteBranch,
                    PermissionCodes.CreateDepartment,
                    PermissionCodes.UpdateDepartment,
                    PermissionCodes.DeleteDepartment,
                    PermissionCodes.CreatePermission,
                    PermissionCodes.UpdatePermission,
                    PermissionCodes.DeletePermission,
                    PermissionCodes.CreateEmployee,
                    PermissionCodes.UpdateEmployee,
                    PermissionCodes.DeleteEmployee,
                    PermissionCodes.CreateTask,
                    PermissionCodes.UpdateTask,
                    PermissionCodes.DeleteTask
                ];
            }

            if (level >= (int)RoleLevelEnum.BranchesManager)
            {
                return
                [
                    PermissionCodes.ViewOwnTasks,
                    PermissionCodes.ViewScopedTasks,
                    PermissionCodes.ViewEmployees,
                    PermissionCodes.ManageManagerScope,
                    PermissionCodes.ApproveLeave,
                    PermissionCodes.RejectLeave,
                    PermissionCodes.ViewScopedReports,
                    PermissionCodes.CreateBranch,
                    PermissionCodes.UpdateBranch,
                    PermissionCodes.CreateTask,
                    PermissionCodes.UpdateTask,
                    PermissionCodes.DeleteTask
                ];
            }

            if (level >= (int)RoleLevelEnum.Manager)
            {
                return
                [
                    PermissionCodes.ViewOwnTasks,
                    PermissionCodes.ViewScopedTasks,
                    PermissionCodes.ViewEmployees,
                    PermissionCodes.ApproveLeave,
                    PermissionCodes.RejectLeave,
                    PermissionCodes.ViewScopedReports,
                    PermissionCodes.CreateEmployee,
                    PermissionCodes.UpdateEmployee,
                    PermissionCodes.CreateTask,
                    PermissionCodes.UpdateTask
                ];
            }

            if (level >= (int)RoleLevelEnum.HR || level >= (int)RoleLevelEnum.Accountant)
            {
                return
                [
                    PermissionCodes.ViewOwnTasks,
                    PermissionCodes.ViewScopedTasks,
                    PermissionCodes.ViewScopedReports,
                    PermissionCodes.ApproveLeave,
                    PermissionCodes.RejectLeave
                ];
            }

            if (level >= (int)RoleLevelEnum.TeamLead)
            {
                return
                [
                    PermissionCodes.ViewOwnTasks,
                    PermissionCodes.CreateTask,
                    PermissionCodes.UpdateTask
                ];
            }

            return [PermissionCodes.ViewOwnTasks];
        }
    }
}
