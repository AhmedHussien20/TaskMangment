using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using TaskMangment.API.Extensions;
using TaskMangment.API.Filters;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Behaviors;
using TaskMangment.Application.Common;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Event;
using TaskMangment.Hangfire.Jobs;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.Caching;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Repositories;
using TaskMangment.Infrastructure.Seeding;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Infrastructure.SignalR;

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

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings")
            );

            builder.Services.AddHangfire(config =>
            config.UseSqlServerStorage(
            builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHangfireServer();

            builder.Services.AddDI();

            // ------------------------------
            // SERVICES
            // ------------------------------
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IWhatsAppService, WhatsAppService>(); 
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            // Domain Events
            builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            // Notification sender 
            builder.Services.AddScoped<INotificationSender, NotificationSender>();
            builder.Services.AddScoped<IEventHandler<TaskAssignedEvent>,TaskAssignedEventHandler>();
            builder.Services.AddScoped<IEventHandler<TaskRequestAddedEvent>, TaskRequestEventHandler>();
            builder.Services.AddScoped<IEventHandler<TaskCommentAddedEvent>, TaskCommentEventHandler>();
            builder.Services.AddScoped<IEventHandler<TaskPenaltyEvent>, TaskPenaltyEventHandler>();



            builder.Services.AddScoped<AuditLogAttribute>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            // SignalR
            builder.Services.AddSignalR();


            builder.Services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
            builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();
            builder.Services.AddScoped<IEventHandler<TaskAssignedEvent>,TaskAssignedEventHandler>();
            builder.Services.AddScoped<IEventHandler<TaskAssignedEvent>, TaskAssignedEmailHandler>();


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
            builder.Services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

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


            var app = builder.Build();

            // ------------------------------
            // PIPELINE
            // ------------------------------

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

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
            //    var jobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

            //    jobClient.Enqueue<ProcessPendingEmailsJob>(
            //        j => j.ExecuteAsync()
            //    );
            //}
            app.UseHangfireDashboard("/hangfire");
            app.UseRouting();
            RecurringJob.AddOrUpdate<ProcessPendingEmailsJob>(
                "process-pending-emails",
                j => j.ExecuteAsync(),
                Cron.Minutely  
            );

            app.Run();
        }
    }
}
