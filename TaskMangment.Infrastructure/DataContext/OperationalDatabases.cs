using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.DataContext
{
    public static class OperationalDatabases
    {
        public static IServiceCollection AddOperationalDatabases(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<TrainingDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", OperationalSchema.Training);
                    sql.CommandTimeout(3600);
                }));

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sql => sql.CommandTimeout(3600)));

            services.AddScoped<AppDbContext>(sp =>
            {
                var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                var schema = http?.User?.FindFirst("Schema")?.Value;
                if (string.IsNullOrWhiteSpace(schema))
                    schema = OperationalSchemaAmbient.Current;

                if (string.Equals(schema, OperationalSchema.Training, StringComparison.OrdinalIgnoreCase))
                    return sp.GetRequiredService<TrainingDbContext>();

                var options = sp.GetRequiredService<DbContextOptions<AppDbContext>>();
                var currentUser = sp.GetRequiredService<ICurrentUserService>();
                return new AppDbContext(options, currentUser);
            });

            return services;
        }
    }
}
