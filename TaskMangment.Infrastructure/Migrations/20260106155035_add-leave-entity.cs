using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addleaveentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "Leaves",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Leaves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_ApprovedById",
                table: "Leaves",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Employees_ApprovedById",
                table: "Leaves",
                column: "ApprovedById",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Employees_ApprovedById",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_ApprovedById",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Leaves");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
