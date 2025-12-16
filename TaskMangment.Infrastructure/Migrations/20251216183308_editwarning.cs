using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editwarning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IssuedEmployeeId",
                table: "Warnings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warnings_IssuedEmployeeId",
                table: "Warnings",
                column: "IssuedEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Warnings_Employees_IssuedEmployeeId",
                table: "Warnings",
                column: "IssuedEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warnings_Employees_IssuedEmployeeId",
                table: "Warnings");

            migrationBuilder.DropIndex(
                name: "IX_Warnings_IssuedEmployeeId",
                table: "Warnings");

            migrationBuilder.DropColumn(
                name: "IssuedEmployeeId",
                table: "Warnings");
        }
    }
}
