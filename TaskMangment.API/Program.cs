using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Serilog; 
using System.Globalization;
using System.Text;
using TaskMangment.API.Extensions;
using TaskMangment.API.Filters; 
using TaskMangment.Hangfire.Jobs;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Seeding;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Infrastructure.SignalR;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Utilities.Localization;

namespace TaskMangment.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            
                var builder = WebApplication.CreateBuilder(args);

                // ------------------------------
                // DATABASE
                // ------------------------------
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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


            //builder.Services.AddHangfire(config =>
            //config.UseSqlServerStorage(
            //builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddHangfireServer();

            builder.Services.AddDI();

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(2);
            });








            builder.Services.AddScoped<AuditLogAttribute>();
                //builder.Services.AddHttpContextAccessor();
                //builder.Services.AddHttpContextAccessor();
                //builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                //builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
                //builder.Services.AddScoped<AttachmentBlobMigrationJob>();


                // SignalR
                builder.Services.AddSignalR();


               

                QuestPDF.Settings.License = LicenseType.Community;


                builder.Services.AddScoped<ProcessPendingEmailsJob>();
                // ------------------------------
                // CORS
                // ------------------------------
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed(origin => true)
                            .AllowCredentials();
                    });
                });

            //Localization
            builder.Services.AddLocalization();
            builder.Services.AddTaskMangmentLocalization();

            builder.Services.AddControllers().AddDataAnnotationsLocalization();

            builder.Services.AddControllers()
                            .AddDataAnnotationsLocalization();

            // ------------------------------
            // JWT AUTH
            // ------------------------------
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                    };

                    // Required for SignalR
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

                // ------------------------------
                // CONTROLLERS
                // ------------------------------
                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
                });

                builder.Configuration.AddUserSecrets<Program>();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = "Task Management API",
                        Version = "v1"
                    });

                    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Enter your JWT token like: Bearer eyJhbGciOiJIUzI1NiIsInR..."
                    });

                    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                    {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                    });
                });

                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<RoleAuthorizationFilter>();
                    options.Filters.Add<ActiveEmployeeFilter>();
                });

                // =======================
                // Configure Serilog (+ optional Seq free UI)
                // =======================
                var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(logDir);

                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Error()
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithMachineName()
                    .WriteTo.File(
                        path: Path.Combine(logDir, "log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
                    );

                var seqEnabled = builder.Configuration.GetValue("Seq:Enabled", false);
                var seqUrl = builder.Configuration["Seq:ServerUrl"];
                if (seqEnabled && !string.IsNullOrWhiteSpace(seqUrl))
                {
                    var seqApiKey = builder.Configuration["Seq:ApiKey"];
                    loggerConfig.WriteTo.Seq(
                        seqUrl,
                        apiKey: string.IsNullOrWhiteSpace(seqApiKey) ? null : seqApiKey);
                }

                Log.Logger = loggerConfig.CreateLogger();

                builder.Host.UseSerilog();
            //builder.Services.AddCaching(builder.Configuration);


            var app = builder.Build();

                app.UseRouting();

            // ------------------------------
            // PIPELINE
            // ------------------------------

            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
                //}

                var supportedCultures = new[]
                {
                new CultureInfo("en"),
                new CultureInfo("ar")
            };

                app.UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture("ar"),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures,

                    RequestCultureProviders = new IRequestCultureProvider[]
                    {
                    new AcceptLanguageHeaderRequestCultureProvider()
                    }
                });
                app.UseMiddleware<ExceptionHandlingMiddleware>();
                app.UseHttpsRedirection();

                app.UseCors("AllowAll");

                //app.UseMiddleware<ExceptionLogMiddleware>();

                app.UseAuthentication();
                app.UseAuthorization();

                // SignalR Hub
                app.MapHub<NotificationHub>("/notifications");

                // API Controllers
                app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await EmployeeTypeSeeder.EnsureSeededAsync(db);

                var migrator = scope.ServiceProvider.GetRequiredService<RolePermissionPackMigrator>();
                await migrator.MigrateAsync();
            }

            //using (var scope = app.Services.CreateScope())
            //{
            //    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            //    await seeder.SeedAsync();
            //}

            //using (var scope = app.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    EmailTemplateSeeder.Seed(db);
            //}

            //using (var scope = app.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    LeaveTypeSeeder.Seed(db);
            //}

            //using (var scope = app.Services.CreateScope())
            //{
            //    var jobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

            //    jobClient.Enqueue<ProcessPendingEmailsJob>(
            //        j => j.ExecuteAsync()
            //    );
            //}
            // app.UseHangfireDashboard("/hangfire");
            //            RecurringJob.AddOrUpdate<ProcessPendingEmailsJob>(
            //                "process-pending-emails",
            //                j => j.ExecuteAsync(),
            //                Cron.Minutely
            //            );

            //            RecurringJob.AddOrUpdate<ITaskDueTodayEmailJob>(
            //                "task-due-today-email-job",
            //                job => job.ExecuteAsync(),
            //                Cron.Daily(8)
            //             );


            //            RecurringJob.AddOrUpdate<AttachmentBlobMigrationJob>(
            //                "attachment-blob-migration",
            //                job => job.ExecuteAsync(),
            //                Cron.Minutely()
            //            );
            //            RecurringJob.AddOrUpdate<ArchiveOverdueTasksJob>(
            //                "archive-overdue-tasks",
            //                job => job.ExecuteAsync(),
            //                Cron.Minutely()
            //);

            //RecurringJob.AddOrUpdate<PenaltyForMissingCommentsJob>(
            //                job => job.ExecuteAsync(),"0 8 * * *",TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time")
            //);



            app.Run();
             
        }
    }
}
