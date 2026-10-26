using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.DataContext
{
    public class TrainingDbContext : AppDbContext
    {
        public TrainingDbContext(
            DbContextOptions<TrainingDbContext> options,
            ICurrentUserService currentUserService)
            : base(options, currentUserService)
        {
        }

        /// <summary>
        /// Zero means the caller is not a trainee (provisioning or Hangfire) and may see every copy.
        /// </summary>
        public int CurrentCompanyId => CurrentUser.CompanyId ?? 0;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            OperationalModel.Configure(builder, OperationalSchema.Training);
            ApplyCompanyFilters(builder);
        }

        private void ApplyCompanyFilters(ModelBuilder builder)
        {
            builder.Entity<Company>().HasQueryFilter(c => CurrentCompanyId == 0 || c.Id == CurrentCompanyId);
            builder.Entity<Area>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Branch>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<CalendarEvent>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Employee>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<PaymentVoucher>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Role>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<WorkTask>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Department>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Student>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Offer>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Course>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Leave>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Job>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Notification>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Attachment>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<TaskComment>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<TaskAssignment>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<TaskCloseRequest>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<TaskExtensionRequest>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<TaskPercentage>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<CourseSubject>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Course.CompanyId == CurrentCompanyId);
            builder.Entity<EmployeeRole>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Employee.CompanyId == CurrentCompanyId);
            builder.Entity<EmployeeFunctionalScope>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Employee.CompanyId == CurrentCompanyId);
            builder.Entity<RolePermission>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Role.CompanyId == CurrentCompanyId);
            builder.Entity<RoleNotificationSource>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Role.CompanyId == CurrentCompanyId);
            builder.Entity<ManagerBranches>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Branch.CompanyId == CurrentCompanyId);
            builder.Entity<OfferAssignment>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Offer.CompanyId == CurrentCompanyId);
            builder.Entity<Warning>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Task.CompanyId == CurrentCompanyId);
            builder.Entity<Deduction>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Employee.CompanyId == CurrentCompanyId);
            builder.Entity<Discount>().HasQueryFilter(e => CurrentCompanyId == 0 || e.Employee.CompanyId == CurrentCompanyId);
            builder.Entity<EmployeeType>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<Permission>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<LeaveType>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
            builder.Entity<AuditLog>().HasQueryFilter(e => CurrentCompanyId == 0 || e.CompanyId == CurrentCompanyId);
        }
    }
}
