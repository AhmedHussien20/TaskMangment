using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addNavigateEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "ManagerID",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResponsibleID",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ManagerID",
                table: "Branches",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ResponsibleID",
                table: "Branches",
                column: "ResponsibleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Employees_ManagerID",
                table: "Branches",
                column: "ManagerID",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Employees_ResponsibleID",
                table: "Branches",
                column: "ResponsibleID",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Employees_ManagerID",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Employees_ResponsibleID",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ManagerID",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ResponsibleID",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ManagerID",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ResponsibleID",
                table: "Branches");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Branches_BranchId",
                table: "Employees",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }
    }
}
