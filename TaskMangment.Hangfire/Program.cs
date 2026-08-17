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

if (args.Contains("--seed-email-templates-only", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine("Email templates seeded successfully.");
    return;
}

// One-shot: schema migrate + email templates + permission catalog/role packs, then exit.
if (args.Contains("--sync-db", StringComparer.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Applying pending EF migrations...");
        await db.Database.MigrateAsync();
        var migrator = scope.ServiceProvider.GetRequiredService<RolePermissionPackMigrator>();
        await migrator.MigrateAsync();
    }
    Console.WriteLine("DB sync completed (schema + email templates + permissions).");
    return;
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
// Quiet (Saudi): 01:00–09:00 daily, and all day Friday.
// =======================
var saudiTimeZone = TimeZoneHelper.GetSaudiArabia();
var quietEnabled = builder.Configuration.GetValue("HangfireQuietHours:Enabled", true);

// Email job: every 5 minutes (not every minute)
// When quiet hours enabled, use the 5-minute cron that respects quiet hours
// When disabled, use simple */5 * * * *
var emailCron = quietEnabled? HangfireQuietHours.CronEvery5MinOutsideQuietHours  // "*/5 0,9-23 * * 0-4,6"
    : "*/5 * * * *";  // Every 5 minutes

// Penalty job: monday to friday only, at 9:30 AM
// Cron: 30 9 * * 1-5 (Monday=1, Tuesday=2, Wednesday=3, Thursday=4, Friday=5)
// This excludes Sunday (0) and Saturday (6)
var penaltyCron = "30 9 * * 1-5";

var archiveCron = quietEnabled ? HangfireQuietHours.CronArchiveOutsideQuietHours
    : Cron.Daily(0, 1);

Console.WriteLine("Saudi timezone for Hangfire cron = " + saudiTimeZone.Id);
Console.WriteLine($"Hangfire quiet hours enabled = {quietEnabled}; email cron = {emailCron}");

RecurringJob.AddOrUpdate<ProcessPendingEmailsJob>(
    "process-pending-emails",
    job => job.ExecuteAsync(),
    emailCron,
    saudiTimeZone);

//RecurringJob.AddOrUpdate<AttachmentBlobMigrationJob>(
//    "attachment-blob-migration",
//    job => job.ExecuteAsync(),
//    Cron.Minutely);

RecurringJob.AddOrUpdate<PenaltyForMissingCommentsJob>(
    "penalty-missing-comments",
    job => job.ExecuteAsync(),
    penaltyCron,
    saudiTimeZone
);

// Close after the due day ends (Saudi calendar): due 27 Jul stays open all of 27 Jul, closes at 00:01 on 28 Jul.
// Skipped on Friday when quiet hours are enabled.
RecurringJob.AddOrUpdate<ArchiveOverdueTasksJob>(
    "archive-overdue-tasks",
    job => job.ExecuteAsync(),
    archiveCron,
    saudiTimeZone
);

RecurringJob.AddOrUpdate<TaskDueTodayEmailsProcessorJob>(
    "task-due-today-email-job",
    job => job.ExecuteAsync(),
    archiveCron,
    saudiTimeZone
);


RecurringJob.AddOrUpdate<SendMonthlyEmployeeDiscountsJob>(
    "send-monthly-employee-discounts",
    job => job.ExecuteAsync(),
    Cron.Monthly(25, 9, 0),
    saudiTimeZone
);

using (var scope = app.Services.CreateScope())
{
    var migrator = scope.ServiceProvider.GetRequiredService<RolePermissionPackMigrator>();
    await migrator.MigrateAsync();
}

app.Run();

 
