using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAndCompanyScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "TaskPercentages",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "TaskExtensionRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "TaskComments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "TaskCloseRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "TaskAssignments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Offers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Leaves",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Attachments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_PlatformUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformUsers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskPercentages_CompanyId",
                table: "TaskPercentages",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskExtensionRequests_CompanyId",
                table: "TaskExtensionRequests",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_CompanyId",
                table: "TaskComments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCloseRequests_CompanyId",
                table: "TaskCloseRequests",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_CompanyId",
                table: "TaskAssignments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_CompanyId",
                table: "Students",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CompanyId",
                table: "Offers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_CompanyId",
                table: "Leaves",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyId",
                table: "Jobs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CompanyId",
                table: "Courses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_CompanyId",
                table: "Attachments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformUsers_Email",
                table: "PlatformUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformUsers_OrganizationId",
                table: "PlatformUsers",
                column: "OrganizationId");

            migrationBuilder.Sql("""
                UPDATE d SET CompanyId = b.CompanyId
                FROM Departments d INNER JOIN Branches b ON b.Id = d.BranchId;

                UPDATE j SET CompanyId = d.CompanyId
                FROM Jobs j INNER JOIN Departments d ON d.Id = j.DepartmentId
                WHERE j.DepartmentId IS NOT NULL;

                UPDATE l SET CompanyId = e.CompanyId
                FROM Leaves l INNER JOIN Employees e ON e.Id = l.EmployeeId;

                UPDATE o SET CompanyId = b.CompanyId
                FROM Offers o INNER JOIN Branches b ON b.Id = o.BranchId
                WHERE o.BranchId IS NOT NULL;

                UPDATE tc SET CompanyId = t.CompanyId
                FROM TaskComments tc INNER JOIN Tasks t ON t.Id = tc.TaskId;

                UPDATE ta SET CompanyId = t.CompanyId
                FROM TaskAssignments ta INNER JOIN Tasks t ON t.Id = ta.TaskId;

                UPDATE tcr SET CompanyId = t.CompanyId
                FROM TaskCloseRequests tcr INNER JOIN Tasks t ON t.Id = tcr.TaskId;

                UPDATE ter SET CompanyId = t.CompanyId
                FROM TaskExtensionRequests ter INNER JOIN Tasks t ON t.Id = ter.TaskId;

                UPDATE tp SET CompanyId = t.CompanyId
                FROM TaskPercentages tp INNER JOIN Tasks t ON t.Id = tp.TaskId;

                UPDATE n SET CompanyId = e.CompanyId
                FROM Notifications n INNER JOIN Employees e ON e.Id = n.UserId;

                UPDATE a SET CompanyId = e.CompanyId
                FROM Attachments a INNER JOIN Employees e ON e.Id = a.UploadedBy
                WHERE a.UploadedBy IS NOT NULL;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Companies_CompanyId",
                table: "Attachments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Companies_CompanyId",
                table: "Courses",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Companies_CompanyId",
                table: "Departments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                table: "Jobs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Companies_CompanyId",
                table: "Leaves",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Companies_CompanyId",
                table: "Notifications",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Companies_CompanyId",
                table: "Offers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Companies_CompanyId",
                table: "Students",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Companies_CompanyId",
                table: "TaskAssignments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCloseRequests_Companies_CompanyId",
                table: "TaskCloseRequests",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_Companies_CompanyId",
                table: "TaskComments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskExtensionRequests_Companies_CompanyId",
                table: "TaskExtensionRequests",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskPercentages_Companies_CompanyId",
                table: "TaskPercentages",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Companies_CompanyId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Companies_CompanyId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Companies_CompanyId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Companies_CompanyId",
                table: "Leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Companies_CompanyId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Companies_CompanyId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Companies_CompanyId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Companies_CompanyId",
                table: "TaskAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskCloseRequests_Companies_CompanyId",
                table: "TaskCloseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_Companies_CompanyId",
                table: "TaskComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskExtensionRequests_Companies_CompanyId",
                table: "TaskExtensionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskPercentages_Companies_CompanyId",
                table: "TaskPercentages");

            migrationBuilder.DropTable(
                name: "PlatformUsers");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_TaskPercentages_CompanyId",
                table: "TaskPercentages");

            migrationBuilder.DropIndex(
                name: "IX_TaskExtensionRequests_CompanyId",
                table: "TaskExtensionRequests");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_CompanyId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_TaskCloseRequests_CompanyId",
                table: "TaskCloseRequests");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_CompanyId",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Students_CompanyId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Offers_CompanyId",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CompanyId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_CompanyId",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_CompanyId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CompanyId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_CompanyId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "TaskPercentages");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "TaskExtensionRequests");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "TaskCloseRequests");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Attachments");
        }
    }
}
