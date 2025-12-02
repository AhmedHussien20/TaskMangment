using CleanArchitectureTemplate.Application.Interfaces.Services;
using CleanArchitectureTemplate.Infrastructure;
using CleanArchitectureTemplate.Infrastructure.Services;
using Hangfire;
using Hangfire.Common;
namespace CleanArchitectureTemplate.HangfireWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHangfire(configuration =>
                configuration.UseSqlServerStorage(
                    builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddHangfireServer();

            builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();

            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

                recurringJobManager.AddOrUpdate(
                    "send-daily-report",
                    Job.FromExpression<IEmailService>(x =>
                        x.SendEmailAsync("Daily Report", "Hello from Hangfire Worker!")),
                    Cron.Minutely(),
                    new RecurringJobOptions()  
                );
            }

            host.Run();
        }
    }
}