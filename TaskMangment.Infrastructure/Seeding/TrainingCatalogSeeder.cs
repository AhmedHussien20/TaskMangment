using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    /// <summary>
    /// Ensures training schema has CompanyId=0 catalog templates that CreateTrainee clones per copy.
    /// </summary>
    public static class TrainingCatalogSeeder
    {
        public static async Task EnsureTemplatesAsync(AppDbContext dbo, TrainingDbContext training)
        {
            if (!await training.Database.CanConnectAsync())
                return;

            await EnsureEmployeeTypesAsync(dbo, training);
            await EnsurePermissionsAsync(dbo, training);
            await EnsureLeaveTypesAsync(dbo, training);
        }

        private static async Task EnsureEmployeeTypesAsync(AppDbContext dbo, TrainingDbContext training)
        {
            // Trainee templates only need Operations (الإدارة).
            var templates = await training.EmployeeTypes.IgnoreQueryFilters()
                .Where(t => t.CompanyId == 0 && !t.IsDeleted)
                .ToListAsync();

            foreach (var extra in templates.Where(t => t.Code != EmployeeTypeCodes.Operations))
            {
                extra.IsDeleted = true;
                extra.DeletedDate = DateTime.UtcNow;
            }

            if (!templates.Any(t => t.Code == EmployeeTypeCodes.Operations))
            {
                var source = await dbo.EmployeeTypes.AsNoTracking()
                    .Where(t => !t.IsDeleted && t.Code == EmployeeTypeCodes.Operations)
                    .OrderBy(t => t.Id)
                    .FirstOrDefaultAsync();

                training.EmployeeTypes.Add(new EmployeeType
                {
                    CompanyId = 0,
                    Code = EmployeeTypeCodes.Operations,
                    NameEn = source?.NameEn ?? "Operations",
                    NameAr = source?.NameAr ?? "الإدارة",
                    SeesAllTypesInBranchScope = source?.SeesAllTypesInBranchScope ?? true,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await training.SaveChangesAsync();
        }

        private static async Task EnsurePermissionsAsync(AppDbContext dbo, TrainingDbContext training)
        {
            if (await training.Permissions.IgnoreQueryFilters()
                    .AnyAsync(p => p.CompanyId == 0 && !p.IsDeleted))
                return;

            var sources = await dbo.Permissions.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Id)
                .ToListAsync();

            if (sources.Count == 0)
            {
                foreach (var entry in PermissionCatalog.All)
                {
                    training.Permissions.Add(new Permission
                    {
                        CompanyId = 0,
                        Code = entry.Code,
                        Name = entry.Name,
                        Description = entry.Description,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }
            else
            {
                foreach (var source in sources)
                {
                    training.Permissions.Add(new Permission
                    {
                        CompanyId = 0,
                        Code = source.Code,
                        Name = source.Name,
                        Description = source.Description,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            await training.SaveChangesAsync();
        }

        private static async Task EnsureLeaveTypesAsync(AppDbContext dbo, TrainingDbContext training)
        {
            if (await training.LeaveTypes.IgnoreQueryFilters()
                    .AnyAsync(l => l.CompanyId == 0 && !l.IsDeleted))
                return;

            var sources = await dbo.LeaveTypes.AsNoTracking()
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.Id)
                .ToListAsync();

            if (sources.Count == 0)
            {
                training.LeaveTypes.AddRange(
                    new LeaveType { CompanyId = 0, NameAr = "إجازة سنوية", NameEn = "Annual Leave", IsPaid = true, MaxDaysPerYear = 21, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة مرضية", NameEn = "Sick Leave", IsPaid = true, MaxDaysPerYear = 30, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة طارئة", NameEn = "Emergency Leave", IsPaid = true, MaxDaysPerYear = 5, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة بدون أجر", NameEn = "Unpaid Leave", IsPaid = false, MaxDaysPerYear = null, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة زواج", NameEn = "Marriage Leave", IsPaid = true, MaxDaysPerYear = 5, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة وفاة", NameEn = "Bereavement Leave", IsPaid = true, MaxDaysPerYear = 5, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة حج", NameEn = "Hajj Leave", IsPaid = true, MaxDaysPerYear = 10, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة وضع", NameEn = "Maternity Leave", IsPaid = true, MaxDaysPerYear = 90, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة أبوة", NameEn = "Paternity Leave", IsPaid = true, MaxDaysPerYear = 3, CreatedDate = DateTime.UtcNow },
                    new LeaveType { CompanyId = 0, NameAr = "إجازة دراسة", NameEn = "Study Leave", IsPaid = false, MaxDaysPerYear = null, CreatedDate = DateTime.UtcNow });
            }
            else
            {
                foreach (var source in sources)
                {
                    training.LeaveTypes.Add(new LeaveType
                    {
                        CompanyId = 0,
                        NameAr = source.NameAr,
                        NameEn = source.NameEn,
                        IsPaid = source.IsPaid,
                        MaxDaysPerYear = source.MaxDaysPerYear,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            await training.SaveChangesAsync();
        }
    }
}
