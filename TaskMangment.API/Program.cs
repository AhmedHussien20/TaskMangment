using TaskMangment.API.Middlewares;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMangment.API.Extensions;
using Microsoft.AspNetCore.Identity;
using TaskMangment.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskMangment.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

             
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

           
            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings")
            );

            
            builder.Services.AddDI();

            
          // builder.Services.AddCaching(builder.Configuration);

             
            //builder.Services.AddScoped<IPermissionService, PermissionService>();
            //builder.Services.AddScoped<IRoleService, RoleService>();

             
            builder.Services.AddControllers();

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
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };
            });






            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

             
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            
            app.UseMiddleware<ExceptionLogMiddleware>();

            app.UseHttpsRedirection();
             
            app.UseAuthentication();

             
            app.UseMiddleware<PermissionMiddleware>();
 
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
