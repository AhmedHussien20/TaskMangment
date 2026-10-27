using Microsoft.EntityFrameworkCore;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.DataContext
{
    public static class OperationalModel
    {
        public static void Configure(ModelBuilder builder, string schema)
        {
            if (string.Equals(schema, OperationalSchema.Training, StringComparison.OrdinalIgnoreCase))
                builder.HasDefaultSchema(OperationalSchema.Training);

            ConfigureRelationships(builder);
            ConfigurePlatform(builder, schema);
            ConfigureTenantCompanyLinks(builder);
            ConfigureTraineeCatalogs(builder, schema);
        }

        /// <summary>
        /// In training every trainee copy owns its own employee types, permissions, leave types and audit log,
        /// so one trainee cannot change another's. Rows with CompanyId 0 are templates copied into each new copy.
        /// dbo keeps its single shared catalog.
        /// </summary>
        private static void ConfigureTraineeCatalogs(ModelBuilder builder, string schema)
        {
            if (!string.Equals(schema, OperationalSchema.Training, StringComparison.OrdinalIgnoreCase))
            {
                builder.Entity<EmployeeType>().Ignore(x => x.CompanyId);
                builder.Entity<Permission>().Ignore(x => x.CompanyId);
                builder.Entity<LeaveType>().Ignore(x => x.CompanyId);
                builder.Entity<AuditLog>().Ignore(x => x.CompanyId);
                builder.Entity<EmailQueue>().Ignore(x => x.CompanyId);
                builder.Entity<EmployeeType>().HasIndex(x => x.Code).IsUnique();
                return;
            }

            builder.Entity<EmployeeType>().HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            builder.Entity<Permission>().HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            builder.Entity<LeaveType>().HasIndex(x => x.CompanyId);
            builder.Entity<AuditLog>().HasIndex(x => x.CompanyId);
            builder.Entity<EmailQueue>().HasIndex(x => x.CompanyId);
        }

        private static void ConfigurePlatform(ModelBuilder builder, string schema)
        {
            if (string.Equals(schema, OperationalSchema.Training, StringComparison.OrdinalIgnoreCase))
            {
                builder.Ignore<Organization>();
                builder.Ignore<PlatformUser>();
                builder.Entity<Company>().Property(c => c.OrganizationId).IsRequired();
                return;
            }

            builder.Entity<Company>().Ignore(c => c.OrganizationId);

            builder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organizations");
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.Name);
            });

            builder.Entity<PlatformUser>(entity =>
            {
                entity.ToTable("PlatformUsers");
                entity.Property(x => x.FullName).HasMaxLength(250).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
                entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(x => x.Role).HasConversion<int>();
                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasOne(x => x.Organization)
                    .WithMany(o => o.Admins)
                    .HasForeignKey(x => x.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureTenantCompanyLinks(ModelBuilder builder)
        {
            LinkCompany<Department>(builder);
            LinkCompany<Student>(builder);
            LinkCompany<Offer>(builder);
            LinkCompany<Course>(builder);
            LinkCompany<Leave>(builder);
            LinkCompany<Job>(builder);
            LinkCompany<Notification>(builder);
            LinkCompany<Attachment>(builder);
            LinkCompany<TaskComment>(builder);
            LinkCompany<TaskAssignment>(builder);
            LinkCompany<TaskCloseRequest>(builder);
            LinkCompany<TaskExtensionRequest>(builder);
            LinkCompany<TaskPercentage>(builder);
        }

        private static void LinkCompany<TEntity>(ModelBuilder builder)
            where TEntity : class
        {
            builder.Entity<TEntity>()
                .HasOne<Company>()
                .WithMany()
                .HasForeignKey("CompanyId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TEntity>().HasIndex("CompanyId");
        }

        private static void ConfigureRelationships(ModelBuilder builder)
        {
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

            builder.Entity<Attachment>()
                .HasIndex(a => new { a.ReferenceId, a.AttachmentType, a.IsDeleted })
                .HasDatabaseName("IX_Attachments_ReferenceId_AttachmentType_IsDeleted");

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

            builder.Entity<RoleNotificationSource>(entity =>
            {
                entity.ToTable("RoleNotificationSources");
                entity.HasOne(x => x.Role)
                    .WithMany(r => r.NotificationSources)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.SourceRole)
                    .WithMany()
                    .HasForeignKey(x => x.SourceRoleId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.RoleId, x.SourceRoleId }).IsUnique();
            });

            builder.Entity<EmployeeType>(entity =>
            {
                entity.ToTable("EmployeeTypes");
                // Unique Code index is per schema: see ConfigureTraineeCatalogs.
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.NameEn).HasMaxLength(100).IsRequired();
                entity.Property(x => x.NameAr).HasMaxLength(100).IsRequired();
            });

            builder.Entity<Employee>()
                .HasOne(e => e.EmployeeType)
                .WithMany(t => t.Employees)
                .HasForeignKey(e => e.EmployeeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EmployeeFunctionalScope>()
                .HasOne(x => x.EmployeeType)
                .WithMany(t => t.FunctionalScopes)
                .HasForeignKey(x => x.EmployeeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Employee>()
                .Property(e => e.EmployeeTypeId)
                .HasColumnName("FunctionCode");

            builder.Entity<EmployeeFunctionalScope>()
                .Property(x => x.EmployeeTypeId)
                .HasColumnName("FunctionCode");
        }
    }
}
