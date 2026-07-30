using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    /// <summary>
    /// Idempotent: ensures the full permission catalog exists, removes unknown junk rows,
    /// and grants level-equivalent packs to existing roles.
    /// </summary>
    public class RolePermissionPackMigrator
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RolePermissionPackMigrator> _logger;

        public RolePermissionPackMigrator(AppDbContext db, ILogger<RolePermissionPackMigrator> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task MigrateAsync()
        {
            await EnsureCanonicalPermissionsAsync();
            await RenameLegacyPermissionsAsync();
            await SoftDeleteUnknownPermissionsAsync();
            await GrantPacksByRoleLevelAsync();
            await _db.SaveChangesAsync();
            _logger.LogInformation("Role permission pack migration completed.");
        }

        /// <summary>
        /// Hard-deletes all RolePermission + Permission rows, reseeds PermissionCatalog,
        /// then grants level packs to every role. Destructive — use intentionally.
        /// </summary>
        public async Task ResetAndSeedAsync()
        {
            _logger.LogWarning("Resetting Permissions and RolePermission tables...");

            // FK order: RolePermission first, then Permissions.
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM RolePermission");
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM Permissions");

            // Reset identity so Ids start clean (SQL Server).
            try
            {
                await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Permissions', RESEED, 0)");
                await _db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('RolePermission', RESEED, 0)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not reseed identity (non-fatal).");
            }

            var now = DateTime.UtcNow;
            foreach (var entry in PermissionCatalog.All)
            {
                _db.Permissions.Add(new Permission
                {
                    Code = entry.Code,
                    Name = entry.Name,
                    Description = entry.Description,
                    CreatedDate = now,
                    IsDeleted = false
                });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} permissions from catalog.", PermissionCatalog.All.Count);

            // After a full wipe, grant packs even if a role previously had custom removals.
            await GrantPacksByRoleLevelAsync(forceRegrant: true);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Permission reset + seed completed.");
        }

        /// <summary>
        /// Restores/creates every catalog code (one active keeper per code) and remaps RolePermissions to keepers.
        /// </summary>
        private async Task EnsureCanonicalPermissionsAsync()
        {
            var now = DateTime.UtcNow;
            var allRows = await _db.Permissions.ToListAsync();
            var byCode = allRows
                .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Id).ToList(), StringComparer.OrdinalIgnoreCase);

            var restored = 0;
            var created = 0;
            var duplicateIds = new List<int>();

            foreach (var entry in PermissionCatalog.All)
            {
                if (!byCode.TryGetValue(entry.Code, out var rows) || rows.Count == 0)
                {
                    _db.Permissions.Add(new Permission
                    {
                        Code = entry.Code,
                        Name = entry.Name,
                        Description = entry.Description,
                        CreatedDate = now
                    });
                    created++;
                    continue;
                }

                // Prefer already-active keeper; otherwise revive lowest Id.
                var keeper = rows.FirstOrDefault(r => !r.IsDeleted) ?? rows[0];
                if (keeper.IsDeleted)
                {
                    keeper.IsDeleted = false;
                    keeper.DeletedDate = null;
                    keeper.ModifiedDate = now;
                    restored++;
                }

                if (!string.Equals(keeper.Name, entry.Name, StringComparison.Ordinal)
                    || !string.Equals(keeper.Description, entry.Description, StringComparison.Ordinal))
                {
                    keeper.Name = entry.Name;
                    keeper.Description = entry.Description;
                    keeper.ModifiedDate = now;
                }

                foreach (var dup in rows.Where(r => r.Id != keeper.Id))
                {
                    if (!dup.IsDeleted)
                    {
                        dup.IsDeleted = true;
                        dup.DeletedDate = now;
                    }
                    duplicateIds.Add(dup.Id);
                }

                if (duplicateIds.Count > 0)
                    await RemapRolePermissionsToKeeperAsync(keeper.Id, duplicateIds, now);

                duplicateIds.Clear();
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "Permission catalog sync: restored {Restored}, created {Created}.",
                restored,
                created);
        }

        private async Task RemapRolePermissionsToKeeperAsync(int keeperId, List<int> duplicateIds, DateTime now)
        {
            if (duplicateIds.Count == 0)
                return;

            var links = await _db.RolePermissions
                .Where(rp => duplicateIds.Contains(rp.PermissionId))
                .ToListAsync();

            if (links.Count == 0)
                return;

            var keeperLinks = await _db.RolePermissions
                .Where(rp => rp.PermissionId == keeperId)
                .ToListAsync();

            var keeperByRole = keeperLinks
                .GroupBy(rp => rp.RoleId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Id).First());

            foreach (var link in links)
            {
                if (keeperByRole.TryGetValue(link.RoleId, out var keeperLink))
                {
                    // Merge assignment onto keeper, then retire the duplicate link.
                    if (link.IsAssigned && !link.IsDeleted)
                    {
                        keeperLink.IsAssigned = true;
                        keeperLink.IsDeleted = false;
                        keeperLink.DeletedDate = null;
                        keeperLink.ModifiedDate = now;
                    }

                    if (!link.IsDeleted)
                    {
                        link.IsDeleted = true;
                        link.DeletedDate = now;
                        link.IsAssigned = false;
                    }
                }
                else
                {
                    // Point this link at the keeper instead of leaving a dangling duplicate id.
                    link.PermissionId = keeperId;
                    link.ModifiedDate = now;
                    if (link.IsAssigned)
                    {
                        link.IsDeleted = false;
                        link.DeletedDate = null;
                    }
                    keeperByRole[link.RoleId] = link;
                }
            }
        }

        /// <summary>
        /// Remaps RolePermissions from legacy codes onto the renamed catalog codes, then soft-deletes old rows.
        /// </summary>
        private async Task RenameLegacyPermissionsAsync()
        {
            var now = DateTime.UtcNow;
            var allRows = await _db.Permissions.ToListAsync();
            var byCode = allRows
                .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Id).ToList(), StringComparer.OrdinalIgnoreCase);

            var renamed = 0;
            foreach (var (oldCode, newCode) in PermissionCodes.LegacyCodeRenames)
            {
                if (string.Equals(oldCode, newCode, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!byCode.TryGetValue(oldCode, out var oldRows) || oldRows.Count == 0)
                    continue;

                if (!byCode.TryGetValue(newCode, out var newRows) || newRows.Count == 0)
                {
                    _logger.LogWarning(
                        "Cannot rename {OldCode} → {NewCode}: target permission missing from catalog sync.",
                        oldCode,
                        newCode);
                    continue;
                }

                var target = newRows.FirstOrDefault(r => !r.IsDeleted) ?? newRows[0];
                if (target.IsDeleted)
                {
                    target.IsDeleted = false;
                    target.DeletedDate = null;
                    target.ModifiedDate = now;
                }

                var oldIds = oldRows.Select(r => r.Id).ToList();
                await RemapRolePermissionsToKeeperAsync(target.Id, oldIds, now);

                foreach (var old in oldRows.Where(r => !r.IsDeleted))
                {
                    old.IsDeleted = true;
                    old.DeletedDate = now;
                    old.ModifiedDate = now;
                    renamed++;
                }
            }

            await _db.SaveChangesAsync();
            if (renamed > 0)
                _logger.LogInformation("Renamed/retired {Count} legacy Permission rows.", renamed);
        }

        /// <summary>
        /// Soft-deletes permission rows (and RolePermission FKs) whose Code is not in the catalog.
        /// </summary>
        private async Task SoftDeleteUnknownPermissionsAsync()
        {
            var now = DateTime.UtcNow;
            var junk = await _db.Permissions
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            junk = junk
                .Where(p => !PermissionCatalog.Codes.Contains(p.Code))
                .ToList();

            if (junk.Count == 0)
                return;

            var junkIds = junk.Select(p => p.Id).ToList();
            foreach (var p in junk)
            {
                p.IsDeleted = true;
                p.DeletedDate = now;
            }

            var links = await _db.RolePermissions
                .Where(rp => junkIds.Contains(rp.PermissionId) && !rp.IsDeleted)
                .ToListAsync();

            foreach (var link in links)
            {
                link.IsDeleted = true;
                link.DeletedDate = now;
                link.IsAssigned = false;
            }

            await _db.SaveChangesAsync();
            _logger.LogWarning(
                "Soft-deleted {Count} unknown Permission rows and {LinkCount} RolePermission links.",
                junk.Count,
                links.Count);
        }

        private async Task GrantPacksByRoleLevelAsync(bool forceRegrant = false)
        {
            await _db.SaveChangesAsync();

            var permissions = (await _db.Permissions
                    .Where(p => !p.IsDeleted)
                    .Select(p => new { p.Code, p.Id })
                    .ToListAsync())
                .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Min(x => x.Id), StringComparer.OrdinalIgnoreCase);

            var roles = await _db.Roles
                .Where(r => !r.IsDeleted)
                .Include(r => r.RolePermissions)
                .ToListAsync();

            foreach (var role in roles)
            {
                var pack = GetPackForLevel(role.Level);
                if (pack.Count == 0)
                    continue;

                var knownPermissionIds = role.RolePermissions
                    .Select(rp => rp.PermissionId)
                    .ToHashSet();

                foreach (var code in pack)
                {
                    if (!permissions.TryGetValue(code, out var permissionId))
                        continue;

                    if (!forceRegrant && knownPermissionIds.Contains(permissionId))
                        continue;

                    if (forceRegrant)
                    {
                        var existing = role.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
                        if (existing != null)
                        {
                            existing.IsAssigned = true;
                            existing.IsDeleted = false;
                            existing.DeletedDate = null;
                            existing.ModifiedDate = DateTime.UtcNow;
                            continue;
                        }
                    }

                    role.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        IsAssigned = true,
                        CreatedDate = DateTime.UtcNow
                    });
                    knownPermissionIds.Add(permissionId);
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
                // Full catalog for admin-level roles.
                return PermissionCatalog.All.Select(e => e.Code).ToArray();
            }

            if (level >= (int)RoleLevelEnum.BranchesManager)
            {
                return
                [
                    PermissionCodes.ViewOwnTasks,
                    PermissionCodes.ViewScopedTasks,
                    PermissionCodes.ViewEmployees,
                    PermissionCodes.ApproveLeave,
                    PermissionCodes.RejectLeave,
                    PermissionCodes.ViewScopedReports,
                    PermissionCodes.CreateBranch,
                    PermissionCodes.UpdateBranch,
                    PermissionCodes.CreateTask,
                    PermissionCodes.UpdateTask,
                    PermissionCodes.DeleteTask,
                    ..PermissionCodes.AssigneeTaskActions,
                    ..PermissionCodes.ManagerTaskActions
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
                    PermissionCodes.EnableEmployee,
                    PermissionCodes.DisableEmployee,
                    PermissionCodes.CreateTask,
                    PermissionCodes.UpdateTask,
                    ..PermissionCodes.AssigneeTaskActions,
                    ..PermissionCodes.ManagerTaskActions
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
                    PermissionCodes.RejectLeave,
                    ..PermissionCodes.AssigneeTaskActions
                ];
            }

            if (level >= (int)RoleLevelEnum.TeamLead)
            {
                return
                [
                    PermissionCodes.ViewOwnTasks,
                    PermissionCodes.CreateTask,
                    PermissionCodes.UpdateTask,
                    PermissionCodes.ApproveTaskRequest,
                    PermissionCodes.RejectTaskRequest,
                    ..PermissionCodes.AssigneeTaskActions
                ];
            }

            return
            [
                PermissionCodes.ViewOwnTasks,
                ..PermissionCodes.AssigneeTaskActions
            ];
        }
    }
}
