using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.AutoMapper;
using TaskMangment.Application.Behaviors.EmailHandlers;
using TaskMangment.Application.Behaviors;
using TaskMangment.Application.Common;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Caching;
using TaskMangment.Infrastructure.Repositories;
using TaskMangment.Infrastructure.Seeding;
using TaskMangment.Utilities.Localization;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.Dashboards.Employee; 
using TaskMangment.Application.Common.Security; 

namespace TaskMangment.Infrastructure.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDI(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
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
            //services.AddSingleton<ICachingService, NoCacheService>();

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
            services.AddScoped<IOnlineUserService, OnlineUserService>();
            services.AddScoped<ITaskPercentageService, TaskPercentageService>();
            services.AddScoped<IReportService, ReportService>();
            // ------------------------------
            // SERVICES
            // ------------------------------
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            // Domain Events
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Notification sender 
            services.AddScoped<INotificationSender, NotificationSender>();
            services.AddScoped<IEventHandler<TaskUnAssignedEvent>, TaskUnAssignedEventHandler>();
            services.AddScoped<IEventHandler<TaskUnAssignedEvent>, TaskUnAssignedEmailHandler>();

            services.AddScoped<IEventHandler<TaskExtensionRequestEvent>, TaskExtenstionRequestEventHandler>();
            services.AddScoped<IEventHandler<TaskCloseRequestEvent>, TaskCloseRequestEventHandler>();
            services.AddScoped<IEventHandler<TaskCommentAddedEvent>, TaskCommentEventHandler>();
            services.AddScoped<IEventHandler<TaskPenaltyEvent>, TaskPenaltyEventHandler>();
            services.AddScoped<IEventHandler<TaskAssignedEvent>, TaskAssignedEventHandler>();
            services.AddScoped<IEventHandler<TaskAssignedEvent>, TaskAssignedEmailHandler>();


            services.AddScoped<IEventHandler<LeaveEvent>, LeaveEmailHandler>();
            services.AddScoped<IEventHandler<LeaveEvent>, LeaveEventHandler>();

            services.AddScoped<IEventHandler<LeaveRejectedEvent>, LeaveRejectedEmailHandler>();
            services.AddScoped<IEventHandler<LeaveRejectedEvent>, LeaveRejectedEventHandler>();

            services.AddScoped<IEventHandler<LeaveApprovedEvent>, LeaveApprovedEventHandler>();
            services.AddScoped<IEventHandler<LeaveApprovedEvent>, LeaveApprovedEmailHandler>();



            // services.AddScoped<IEventHandler<TaskRequestAddedEvent>, TaskRequestEmailHandler>();

            services.AddScoped<IEventHandler<TaskWarningEvent>, TaskWarningEventHandler>();
            services.AddScoped<IEventHandler<TaskExtensionRequestEvent>, TaskExtenstionRequestEmailHandler>();
            services.AddScoped<IEventHandler<TaskCloseRequestEvent>, TaskCloseRequestEmailHandler>();
            services.AddScoped<IEventHandler<TaskPenaltyEvent>, TaskPenaltyEmailHandler>();
            services.AddScoped<IEventHandler<TaskWarningEvent>, TaskWarningEmailHandler>();
            services.AddScoped<IEventHandler<TaskCommentAddedEvent>, TaskCommentEmailHandler>();
            services.AddScoped<IEventHandler<OfferSentEvent>, OfferSentEmailHandler>();
            services.AddScoped<IEventHandler<TaskCloseApproveEvent>, TaskCloseApproveEventHandler>();
            services.AddScoped<IEventHandler<TaskCloseApproveEvent>, TaskCloseApproveEmailHandler>();
            services.AddScoped<IEventHandler<TaskExtendApproveEvent>, TaskExtendApproveEventHandler>();
            services.AddScoped<IEventHandler<TaskExtendApproveEvent>, TaskExtendApproveEmailHandler>();

            services.AddScoped<IEventHandler<TaskAchievePercentEvent>, TaskAchievePercentEmailHandler>();
            services.AddScoped<IEventHandler<TaskAchievePercentEvent>, TaskAchievePercentEventHandler>();
            services.AddScoped<IEventHandler<PublicHolidayEvent>, PublicHolidayEmailHandler>();


            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            services.AddHttpContextAccessor();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<AttachmentBlobMigrationJob>();


            // SignalR
            services.AddSignalR();


            services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailQueueService, EmailQueueService>();
            services.AddScoped<ITaskDueTodayEmailJob, TaskDueTodayEmailJob>();
            services.AddScoped<IEmployeeDashboardService, EmployeeDashboardService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();
            services.AddScoped<IOfferSendService, OfferSendService>();
            services.AddScoped<IChartService, ChartService>();
            services.AddScoped<IUserAccessContextProvider, UserAccessContextProvider>();
            
            services.AddScoped<IPermissionChecker, PermissionChecker>();
            services.AddScoped<IGetHigherManager, GetHigherManager>();

            services.AddScoped<ICacheInvalidator, CacheInvalidator>();


            return services;
        }
    }
}
