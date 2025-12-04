using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addEmployeeAtTaskClose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestedByEmployeeId",
                table: "TaskCloseRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskCloseRequests_RequestedByEmployeeId",
                table: "TaskCloseRequests",
                column: "RequestedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCloseRequests_Employees_RequestedByEmployeeId",
                table: "TaskCloseRequests",
                column: "RequestedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCloseRequests_Employees_RequestedByEmployeeId",
                table: "TaskCloseRequests");

            migrationBuilder.DropIndex(
                name: "IX_TaskCloseRequests_RequestedByEmployeeId",
                table: "TaskCloseRequests");

            migrationBuilder.DropColumn(
                name: "RequestedByEmployeeId",
                table: "TaskCloseRequests");
        }
    }
}
