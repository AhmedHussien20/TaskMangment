using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.AutoMapper;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.Caching;
using TaskMangment.Infrastructure.Repositories;
using TaskMangment.Infrastructure.Seeding;
using TaskMangment.Utilities.Localization;

namespace TaskMangment.Infrastructure.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDI(this IServiceCollection services)
        {
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<LocalizationService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRoleAssignmentService, RoleAssignmentService>();
            services.AddScoped<IRolePermissionService, RolePermissionService>();

            services.AddScoped<ITaskService, TaskService>();
            services.AddSingleton<ICachingService, NoCacheService>();

            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<IPaymentVoucherService, PaymentVoucherService>();
            services.AddScoped<ITaskCommentService, TaskCommentService>();
            services.AddScoped<ITaskExtensionRequestsService, TaskExtensionRequestService>();
            services.AddScoped<ITaskCloseRequestService, TaskCloseRequestService>();
            services.AddScoped<ITaskWarningService, TaskWarningService>();
             
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<ICalenderEventsService, CalendarEventService>();
            services.AddScoped<ITaskDiscountService, TaskDiscountService>();
            services.AddScoped<DataSeeder>();









            return services;
        }
    }
}
