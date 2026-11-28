using Microsoft.EntityFrameworkCore;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    public static class EmployeeTypeSeeder
    {
        public static async Task EnsureSeededAsync(AppDbContext db)
        {
            var seeds = new[]
            {
                new EmployeeType
                {
                    Id = 1,
                    Code = EmployeeTypeCodes.Operations,
                    NameEn = "Operations",
                    NameAr = "عمليات",
                    SeesAllTypesInBranchScope = true,
                    CreatedDate = DateTime.UtcNow
                },
                new EmployeeType
                {
                    Id = 2,
                    Code = EmployeeTypeCodes.Accounting,
                    NameEn = "Accounting",
                    NameAr = "محاسبة",
                    SeesAllTypesInBranchScope = false,
                    CreatedDate = DateTime.UtcNow
                },
                new EmployeeType
                {
                    Id = 3,
                    Code = EmployeeTypeCodes.HR,
                    NameEn = "HR",
                    NameAr = "موارد بشرية",
                    SeesAllTypesInBranchScope = false,
                    CreatedDate = DateTime.UtcNow
                },
                new EmployeeType
                {
                    Id = 4,
                    Code = EmployeeTypeCodes.GeneralAffairs,
                    NameEn = "General Affairs",
                    NameAr = "شؤون عامة",
                    SeesAllTypesInBranchScope = false,
                    CreatedDate = DateTime.UtcNow
                }
            };

            foreach (var seed in seeds)
            {
                var existing = await db.EmployeeTypes
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == seed.Id || x.Code == seed.Code);

                if (existing == null)
                {
                    // Allow explicit Ids for seed rows.
                    await db.Database.OpenConnectionAsync();
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT EmployeeTypes ON");
                        db.EmployeeTypes.Add(seed);
                        await db.SaveChangesAsync();
                        await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT EmployeeTypes OFF");
                    }
                    finally
                    {
                        await db.Database.CloseConnectionAsync();
                    }
                }
                else if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedDate = null;
                    existing.Code = seed.Code;
                    existing.NameEn = seed.NameEn;
                    existing.NameAr = seed.NameAr;
                    existing.SeesAllTypesInBranchScope = seed.SeesAllTypesInBranchScope;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
