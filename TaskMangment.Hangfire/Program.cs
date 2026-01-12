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
using Microsoft.AspNetCore.Localization;
using System.Globalization;

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


builder.Services.AddLocalization();


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

var app = builder.Build();
var defaultCulture = new CultureInfo("ar");

CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var supportedCultures = new[]
{
    new CultureInfo("ar"),
    new CultureInfo("en")
};

Console.WriteLine("Current UI Culture = " + CultureInfo.CurrentUICulture.Name);

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ar"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
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

//RecurringJob.AddOrUpdate<AttachmentBlobMigrationJob>(
//    "attachment-blob-migration",
//    job => job.ExecuteAsync(),
//    Cron.Minutely);

RecurringJob.AddOrUpdate<PenaltyForMissingCommentsJob>(
      "penalty-missing-comments",
    job => job.ExecuteAsync(),
    Cron.Hourly
);

RecurringJob.AddOrUpdate<ArchiveOverdueTasksJob>(
               "archive-overdue-tasks",
               job => job.ExecuteAsync(),
               Cron.Hourly
               );
RecurringJob.AddOrUpdate<TaskDueTodayEmailsProcessorJob>(
               "task-due-today-email-job",
               job => job.ExecuteAsync(),
               Cron.Minutely
               );


app.Run();

 
