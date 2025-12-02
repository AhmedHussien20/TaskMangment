using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Services;
using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

// 1️⃣ DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2️⃣ EmailService
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);
builder.Services.AddTransient<IEmailService, EmailService>();
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));

// 3️⃣ Hangfire
builder.Services.AddHangfire(cfg =>
    cfg.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddHangfireServer();

var app = builder.Build();

// 4️⃣ Dashboard
app.UseHangfireDashboard("/hangfire");

// 5️⃣ Recurring Job
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate(
        "send-daily-report",
        Job.FromExpression<IEmailService>(x =>
            x.SendEmailAsync("Daily Report", "Hello from Hangfire Worker!")),
        Cron.Minutely()
    );
}

app.Run();