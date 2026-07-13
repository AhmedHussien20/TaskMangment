using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.DataContext
{
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = ResolveConfigPath();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString =
                config.GetConnectionString("DefaultConnection");

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(connectionString);

            return new AppDbContext(
                builder.Options,
                new DesignTimeCurrentUserService()
            );
        }

        private static string ResolveConfigPath()
        {
            var current = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                current,
                Path.Combine(current, "TaskMangment.API"),
                Path.Combine(current, "..", "TaskMangment.API"),
                Path.Combine(current, "..", "..", "TaskMangment.API"),
            };

            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(Path.Combine(fullPath, "appsettings.json")))
                    return fullPath;
            }

            throw new InvalidOperationException("Could not locate appsettings.json for design-time DbContext creation.");
        }
    }

}
