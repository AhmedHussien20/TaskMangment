using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TaskMangment.Infrastructure.DataContext
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Get the directory of the API project (where appsettings.json lives)
            var basePath = Directory.GetCurrentDirectory();

            // If running from Infrastructure, move up to API project
            if (!File.Exists(Path.Combine(basePath, "appsettings.Azure.json")))
            {
                basePath = Directory.GetParent(basePath)
                                    ?.Parent?
                                    .Parent?
                                    .FullName!;
            }

            // Read appsettings.json normally
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Azure.json", optional: false)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}
