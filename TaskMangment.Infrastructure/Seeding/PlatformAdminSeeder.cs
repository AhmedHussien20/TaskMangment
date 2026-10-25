using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Seeding
{
    public static class PlatformAdminSeeder
    {
        public static async Task EnsureAsync(AppDbContext dbo, IConfiguration configuration)
        {
            if (await dbo.PlatformUsers.AnyAsync())
                return;

            var email = configuration["Platform:SeedAdminEmail"];
            var password = configuration["Platform:SeedAdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            dbo.PlatformUsers.Add(new PlatformUser
            {
                FullName = configuration["Platform:SeedAdminName"] ?? "Platform Admin",
                Email = email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = PlatformRole.PlatformAdmin,
                IsActive = true
            });

            await dbo.SaveChangesAsync();
        }
    }
}
