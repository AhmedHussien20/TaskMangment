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
                Role = PlatformRole.SuperAdmin,
                IsActive = true
            });

            await dbo.SaveChangesAsync();
        }

        public static async Task EnsureSuperAdminAsync(AppDbContext dbo, IConfiguration configuration)
        {
            var email = configuration["Platform:SeedAdminEmail"]?.Trim();
            var password = configuration["Platform:SeedAdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var existing = await dbo.PlatformUsers.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            if (existing != null)
            {
                if (existing.Role != PlatformRole.SuperAdmin)
                {
                    existing.Role = PlatformRole.SuperAdmin;
                    await dbo.SaveChangesAsync();
                }
                return;
            }

            dbo.PlatformUsers.Add(new PlatformUser
            {
                FullName = configuration["Platform:SeedAdminName"] ?? "Platform Admin",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = PlatformRole.SuperAdmin,
                IsActive = true
            });
            await dbo.SaveChangesAsync();
        }
    }
}
