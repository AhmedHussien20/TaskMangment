using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
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
        public DbSet<LeaveType> leaveTypes { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferAssignment> OfferAssignments { get; set; }
        public DbSet<PaymentVoucher> PaymentVouchers { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<WorkTask> Tasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<TaskCloseRequest> TaskCloseRequests { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskExtensionRequest> TaskExtensionRequests { get; set; }
        public DbSet<Warning> Warnings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
                 .OnDelete(DeleteBehavior.Cascade);
             
            builder.Entity<Attachment>()
                 .HasOne(a => a.Task)
                 .WithMany(t => t.Attachments)
                 .HasForeignKey(a => a.TaskId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attachment>()
                 .HasOne(a => a.Comment)
                 .WithMany(c => c.Attachments)
                 .HasForeignKey(a => a.CommentId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attachment>()
                 .HasOne(a => a.Voucher)
                 .WithMany(v => v.Attachments)
                 .HasForeignKey(a => a.VoucherId)
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
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskCloseRequest>()
                 .HasOne(r => r.TaskAssignment)
                 .WithMany(a => a.CloseRequests)
                 .HasForeignKey(r => r.TaskAssignmentId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Warning>()
                 .HasOne(w => w.TaskAssignment)
                 .WithMany(a => a.Warnings)
                 .HasForeignKey(w => w.TaskAssignmentId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PaymentVoucher>()
                 .HasMany(v => v.Attachments)
                 .WithOne(a => a.Voucher)
                 .HasForeignKey(a => a.VoucherId)
                 .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
