using Microsoft.EntityFrameworkCore;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    public static class TrainingCatalogSeeder
    {
        public static async Task CopyIfEmptyAsync(AppDbContext dbo, TrainingDbContext training)
        {
            if (!await training.Database.CanConnectAsync())
                return;

            if (await training.Permissions.IgnoreQueryFilters().AnyAsync())
                return;

            await training.Database.ExecuteSqlRawAsync("""
                SET IDENTITY_INSERT training.Permissions ON;
                INSERT INTO training.Permissions
                    (Id, Code, Name, Description, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted)
                SELECT Id, Code, Name, Description, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted
                FROM dbo.Permissions;
                SET IDENTITY_INSERT training.Permissions OFF;

                SET IDENTITY_INSERT training.EmployeeTypes ON;
                INSERT INTO training.EmployeeTypes
                    (Id, Code, NameEn, NameAr, SeesAllTypesInBranchScope, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted)
                SELECT Id, Code, NameEn, NameAr, SeesAllTypesInBranchScope, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted
                FROM dbo.EmployeeTypes;
                SET IDENTITY_INSERT training.EmployeeTypes OFF;

                SET IDENTITY_INSERT training.EmailTemplates ON;
                INSERT INTO training.EmailTemplates
                    (Id, [Key], SubjectTemplate, BodyTemplate, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted)
                SELECT Id, [Key], SubjectTemplate, BodyTemplate, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted
                FROM dbo.EmailTemplates;
                SET IDENTITY_INSERT training.EmailTemplates OFF;

                SET IDENTITY_INSERT training.LeaveTypes ON;
                INSERT INTO training.LeaveTypes
                    (Id, NameAr, NameEn, IsPaid, MaxDaysPerYear, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted)
                SELECT Id, NameAr, NameEn, IsPaid, MaxDaysPerYear, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, DeletedBy, DeletedDate, IsDeleted
                FROM dbo.LeaveTypes;
                SET IDENTITY_INSERT training.LeaveTypes OFF;
                """);
        }
    }
}
