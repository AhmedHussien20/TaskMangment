using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System.Globalization;
using TaskMangment.Application.Common;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Hangfire;
using TaskMangment.Hangfire.Jobs;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Seeding;
using TaskMangment.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOptions();
builder.Services.AddHttpClient();

builder.Services.ConfigureEmailSettings(builder.Configuration);

builder.Services.Configure<WhatsAppSettings>(opts =>
{
    builder.Configuration.GetSection("WhatsApp").Bind(opts);
    var instanceId = Environment.GetEnvironmentVariable("WHATSAPP_INSTANCE_ID");
    var token = Environment.GetEnvironmentVariable("WHATSAPP_API_TOKEN");
    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
    if (!string.IsNullOrWhiteSpace(instanceId)) opts.InstanceId = instanceId;
    if (!string.IsNullOrWhiteSpace(token)) opts.Token = token;
    if (!string.IsNullOrWhiteSpace(frontendUrl)) opts.FrontendUrl = frontendUrl;
});
builder.Services.AddDI();
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

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    TaskMangment.Infrastructure.Seeding.EmailTemplateSeeder.Seed(db);
}

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

var saudiTimeZone = TimeZoneHelper.GetSaudiArabia();
Console.WriteLine("Saudi timezone for Hangfire cron = " + saudiTimeZone.Id);

RecurringJob.AddOrUpdate<PenaltyForMissingCommentsJob>(
    "penalty-missing-comments",
    job => job.ExecuteAsync(),
    Cron.Daily(8, 0),
    saudiTimeZone
);

RecurringJob.AddOrUpdate<ArchiveOverdueTasksJob>(
    "archive-overdue-tasks",
    job => job.ExecuteAsync(),
    Cron.Daily(8, 0),
    saudiTimeZone
);

RecurringJob.AddOrUpdate<TaskDueTodayEmailsProcessorJob>(
               "task-due-today-email-job",
               job => job.ExecuteAsync(),
               Cron.Minutely
               );

RecurringJob.AddOrUpdate<SendMonthlyEmployeeDiscountsJob>(
    "send-monthly-employee-discounts",
    job => job.ExecuteAsync(),
    Cron.Monthly(25, 8, 0),
    saudiTimeZone
);

using (var scope = app.Services.CreateScope())
{
    var migrator = scope.ServiceProvider.GetRequiredService<RolePermissionPackMigrator>();
    await migrator.MigrateAsync();
}

app.Run();

 
