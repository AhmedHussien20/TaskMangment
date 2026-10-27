using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.DataContext
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ICurrentUserService currentUserService)
            : this((DbContextOptions)options, currentUserService)
        {
        }

        protected AppDbContext(
            DbContextOptions options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        protected ICurrentUserService CurrentUser => _currentUserService;

        public override int SaveChanges()
        {
            StampAuditAndCompany();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampAuditAndCompany();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void StampAuditAndCompany()
        {
            var userId = _currentUserService.UserId;
            StampCompanyId();

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.ModifiedDate = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.Entity.DeletedBy = userId;
                    entry.Entity.DeletedDate = DateTime.UtcNow;
                }
            }
        }

        private void StampCompanyId()
        {
            // Prefer JWT company; Hangfire / DevAlert background work use OperationalCompanyAmbient.
            var userCompany = _currentUserService.CompanyId ?? OperationalCompanyAmbient.Current;
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
            {
                var property = entry.Properties.FirstOrDefault(p =>
                    p.Metadata.Name == "CompanyId" && p.Metadata.ClrType == typeof(int));
                if (property == null)
                    continue;

                if (property.CurrentValue is int existing && existing > 0)
                    continue;

                var resolved = userCompany.GetValueOrDefault();
                if (resolved == 0)
                    resolved = ResolveCompanyId(entry);

                if (resolved > 0)
                    property.CurrentValue = resolved;
            }
        }

        private int ResolveCompanyId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            if (TryInt(entry, "EmployeeId", out var employeeId))
            {
                var company = Employees.IgnoreQueryFilters().Where(e => e.Id == employeeId).Select(e => (int?)e.CompanyId).FirstOrDefault();
                if (company.GetValueOrDefault() > 0)
                    return company!.Value;
            }

            if (TryInt(entry, "UserId", out var userId))
            {
                var company = Employees.IgnoreQueryFilters().Where(e => e.Id == userId).Select(e => (int?)e.CompanyId).FirstOrDefault();
                if (company.GetValueOrDefault() > 0)
                    return company!.Value;
            }

            if (TryInt(entry, "TaskId", out var taskId))
            {
                var company = Tasks.IgnoreQueryFilters().Where(t => t.Id == taskId).Select(t => (int?)t.CompanyId).FirstOrDefault();
                if (company.GetValueOrDefault() > 0)
                    return company!.Value;
            }

            if (TryInt(entry, "BranchId", out var branchId))
            {
                var company = Branches.IgnoreQueryFilters().Where(b => b.Id == branchId).Select(b => (int?)b.CompanyId).FirstOrDefault();
                if (company.GetValueOrDefault() > 0)
                    return company!.Value;
            }

            return 0;
        }

        private static bool TryInt(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName, out int value)
        {
            value = 0;
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
            if (property?.CurrentValue is int number && number > 0)
            {
                value = number;
                return true;
            }

            return false;
        }

        public DbSet<Area> Areas { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSubject> CourseSubjects { get; set; }
        public DbSet<Deduction> Deductions { get; set; }
        public DbSet<DeductionType> DeductionTypes { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferAssignment> OfferAssignments { get; set; }
        public DbSet<PaymentVoucher> PaymentVouchers { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<WorkTask> Tasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<TaskCloseRequest> TaskCloseRequests { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskExtensionRequest> TaskExtensionRequests { get; set; }
        public DbSet<TaskPercentage> TaskPercentages { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailQueue> EmailQueue { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<WhatsAppQueue> WhatsAppQueue { get; set; }
        public DbSet<ManagerBranches> managerBranches { get; set; }
        public DbSet<EmployeeFunctionalScope> EmployeeFunctionalScopes { get; set; }
        public DbSet<EmployeeType> EmployeeTypes { get; set; }
        public DbSet<RoleNotificationSource> RoleNotificationSources { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<PlatformUser> PlatformUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            OperationalModel.Configure(builder, OperationalSchema.Dbo);
        }
    }
}
