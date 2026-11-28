using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleEmployeeTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeTypeId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_EmployeeTypeId",
                table: "Roles",
                column: "EmployeeTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_EmployeeTypes_EmployeeTypeId",
                table: "Roles",
                column: "EmployeeTypeId",
                principalTable: "EmployeeTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_EmployeeTypes_EmployeeTypeId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_EmployeeTypeId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "EmployeeTypeId",
                table: "Roles");
        }
    }
}
