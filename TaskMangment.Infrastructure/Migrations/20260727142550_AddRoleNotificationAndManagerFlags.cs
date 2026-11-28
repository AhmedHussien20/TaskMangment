using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleNotificationAndManagerFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanBeBranchManager",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "NotificationScope",
                table: "Roles",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "RoleNotificationSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    SourceRoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleNotificationSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleNotificationSources_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleNotificationSources_Roles_SourceRoleId",
                        column: x => x.SourceRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleNotificationSources_RoleId_SourceRoleId",
                table: "RoleNotificationSources",
                columns: new[] { "RoleId", "SourceRoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleNotificationSources_SourceRoleId",
                table: "RoleNotificationSources",
                column: "SourceRoleId");

            // Legacy Level 70 roles can be assigned as Branch.ManagerID
            migrationBuilder.Sql(
                "UPDATE Roles SET CanBeBranchManager = 1 WHERE [Level] = 70 AND IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleNotificationSources");

            migrationBuilder.DropColumn(
                name: "CanBeBranchManager",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "NotificationScope",
                table: "Roles");
        }
    }
}
