using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.DataContext
{
    public class TrainingDbContextFactory : IDesignTimeDbContextFactory<TrainingDbContext>
    {
        public TrainingDbContext CreateDbContext(string[] args)
        {
            var current = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                current,
                Path.Combine(current, "TaskMangment.API"),
                Path.Combine(current, "..", "TaskMangment.API"),
                Path.Combine(current, "..", "..", "TaskMangment.API"),
            };

            string? basePath = null;
            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(Path.Combine(fullPath, "appsettings.json")))
                {
                    basePath = fullPath;
                    break;
                }
            }

            if (basePath == null)
                throw new InvalidOperationException("Could not locate appsettings.json for design-time DbContext creation.");

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<TrainingDbContext>();
            builder.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", OperationalSchema.Training);
                    sql.CommandTimeout(3600);
                });

            return new TrainingDbContext(builder.Options, new DesignTimeCurrentUserService());
        }
    }
}
