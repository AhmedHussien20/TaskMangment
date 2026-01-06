using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Hangfire.Jobs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.Services;
using Hangfire.Dashboard;
using TaskMangment.Hangfire;
using TaskMangment.Infrastructure;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOptions();

builder.Services.Configure<EmailSettings>(
                  builder.Configuration.GetSection("EmailSettings")
              );
 builder.Services.AddDI();
builder.Services.Configure<BlobStorageService>(builder.Configuration.GetSection("Blob"));
// =======================
// Services
// =======================
builder.Services.AddScoped<ICurrentUserService, HangfireCurrentUserService>();
builder.Services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
 builder.Services.AddLocalization(option => option.ResourcesPath = "Resources");
IStringLocalizer<TaskDiscountService> localizer = null;
// =======================
// Database
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// Hangfire
// =======================
builder.Services.AddHangfire(config =>
{
    config
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
});

builder.Services.AddHangfireServer();

// =======================
// DI
// =======================
builder.Services.AddDI();

var app = builder.Build();

// =======================
// Middleware
// =======================
app.UseRouting();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Task Management - Hangfire",
    IsReadOnlyFunc = _ => false,
    Authorization = new[] { new HangfireAllowAllFilter() }
});

// =======================
// Jobs
// =======================
RecurringJob.AddOrUpdate<ProcessPendingEmailsJob>(
    "process-pending-emails",
    job => job.ExecuteAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<AttachmentBlobMigrationJob>(
    "attachment-blob-migration",
    job => job.ExecuteAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<PenaltyForMissingCommentsJob>(
      "penalty-missing-comments",
    job => job.ExecuteAsync(),
    Cron.Minutely 
);

RecurringJob.AddOrUpdate<ArchiveOverdueTasksJob>(
               "archive-overdue-tasks",
               job => job.ExecuteAsync(),
               Cron.Minutely()
               );



app.Run();

 
