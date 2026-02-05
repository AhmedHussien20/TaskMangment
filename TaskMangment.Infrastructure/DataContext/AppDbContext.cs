using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
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
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public override async Task<int> SaveChangesAsync( CancellationToken cancellationToken = default)
        {
            var userId = 
                _currentUserService.UserId;

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

            return await base.SaveChangesAsync(cancellationToken);
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


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermission");

                entity.HasKey(rp => rp.Id); 

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Branch>()
                    .HasMany(b => b.Employees)
                    .WithOne(e => e.Branch)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Branch>()
                    .HasOne(b => b.Manager)
                    .WithMany()
                    .HasForeignKey(b => b.ManagerID)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Branch>()
                .HasOne(b => b.Responsible)
                .WithMany()
                .HasForeignKey(b => b.ResponsibleID)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Company>()
                   .HasOne(c => c.FinancialManager)
                   .WithMany()
                   .HasForeignKey(c => c.FinancialManagerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Company>()
                 .HasOne(c => c.TechnicalManager)
                 .WithMany()
                 .HasForeignKey(c => c.TechnicalManagerId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkTask>()
                 .HasOne(t => t.AssignedBy)
                 .WithMany()
                 .HasForeignKey(t => t.AssignedByEmployeeId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkTask>()
                 .HasOne(t => t.CreatedBy)
                 .WithMany()
                 .HasForeignKey(t => t.CreatedByEmployeeId)
                 .OnDelete(DeleteBehavior.Restrict);
             
            builder.Entity<TaskAssignment>()
                 .HasOne(a => a.Employee)
                 .WithMany()
                 .HasForeignKey(a => a.EmployeeId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskAssignment>()
                 .HasOne(a => a.Task)
                 .WithMany(t => t.Assignments)
                 .HasForeignKey(a => a.TaskId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attachment>()
                 .HasOne(a => a.UploadedByEmployee)
                 .WithMany()
                 .HasForeignKey(a => a.UploadedBy)
                 .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<TaskExtensionRequest>()
                 .HasOne(r => r.TaskAssignment)
                 .WithMany(a => a.ExtensionRequests)
                 .HasForeignKey(r => r.TaskAssignmentId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskCloseRequest>()
                 .HasOne(r => r.TaskAssignment)
                 .WithMany(a => a.CloseRequests)
                 .HasForeignKey(r => r.TaskAssignmentId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskCloseRequest>()
                    .HasOne(tcr => tcr.Task)
                    .WithMany(t => t.CloseRequests)  
                    .HasForeignKey(tcr => tcr.TaskId)
                    .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Warning>()
                 .HasOne(w => w.TaskAssignment)
                 .WithMany(a => a.Warnings)
                 .HasForeignKey(w => w.TaskAssignmentId)
                 .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Message)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.HasIndex(x => new { x.UserId, x.IsRead });
            });

            builder.Entity<EmailQueue>(entity =>
            {
                entity.ToTable("EmailQueue");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.ToEmail)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(x => x.TemplateKey)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Status)
                      .HasConversion<int>()
                      .IsRequired();

                entity.Property(x => x.ReferenceType)
                      .HasConversion<int>();

                entity.HasIndex(x => new { x.Status, x.ScheduledAt });
                entity.HasIndex(x => x.UserId);

            });

            builder.Entity<EmailTemplate>(entity =>
            {
                entity.ToTable("EmailTemplates");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Key)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.SubjectTemplate)
                      .IsRequired()
                      .HasMaxLength(300);

                entity.Property(x => x.BodyTemplate)
                      .IsRequired();

                entity.HasIndex(x => x.Key)
                      .IsUnique();
            });

            builder.Entity<WhatsAppQueue>(entity =>
            {
                entity.ToTable("WhatsAppQueue");

                entity.HasKey(x => x.Id); 
                entity.Property(x => x.Message)
                      .IsRequired()
                      .HasMaxLength(1000);             
            });

            builder.Entity<ManagerBranches>()
    .HasOne(x => x.Manager)
    .WithMany()
    .HasForeignKey(x => x.ManagerId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ManagerBranches>()
                .HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ManagerBranches>()
                .HasIndex(x => new { x.ManagerId, x.BranchId })
                .IsUnique();

        }

    }
}
