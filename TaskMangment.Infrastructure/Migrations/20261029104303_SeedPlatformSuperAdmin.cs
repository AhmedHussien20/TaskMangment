using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlatformSuperAdmin : Migration
    {
        /// <summary>
        /// Bootstrap SuperAdmin in dbo.PlatformUsers (BCrypt of Platform@12345).
        /// Matches Platform:SeedAdmin* in appsettings.json.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Role: SuperAdmin = 3. Idempotent if email already exists (including soft-deleted unique index).
            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM [dbo].[PlatformUsers] WHERE [Email] = N'platform.admin@local'
                )
                BEGIN
                    INSERT INTO [dbo].[PlatformUsers]
                    (
                        [FullName],
                        [Email],
                        [PasswordHash],
                        [Role],
                        [OrganizationId],
                        [IsActive],
                        [CreatedBy],
                        [CreatedDate],
                        [ModifiedBy],
                        [ModifiedDate],
                        [DeletedBy],
                        [DeletedDate],
                        [IsDeleted]
                    )
                    VALUES
                    (
                        N'Platform Admin',
                        N'platform.admin@local',
                        N'$2a$11$ZQYVevFmr0/KpK/obR73i.Y80JyOIROagG7RvOelW4IdPnAIN/ria',
                        3,
                        NULL,
                        1,
                        NULL,
                        SYSUTCDATETIME(),
                        NULL,
                        NULL,
                        NULL,
                        NULL,
                        0
                    );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [dbo].[PlatformUsers]
                WHERE [Email] = N'platform.admin@local'
                  AND [Role] = 3
                  AND [OrganizationId] IS NULL;
                """);
        }
    }
}
