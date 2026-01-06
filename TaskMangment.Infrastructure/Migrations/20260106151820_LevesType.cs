using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LevesType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_leaveTypes_LeaveTypeId",
                table: "Leaves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_leaveTypes",
                table: "leaveTypes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "leaveTypes");

            migrationBuilder.RenameTable(
                name: "leaveTypes",
                newName: "LeaveTypes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "LeaveTypes",
                newName: "NameEn");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "LeaveTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxDaysPerYear",
                table: "LeaveTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "LeaveTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveTypes",
                table: "LeaveTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_LeaveTypes_LeaveTypeId",
                table: "Leaves",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_LeaveTypes_LeaveTypeId",
                table: "Leaves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveTypes",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "MaxDaysPerYear",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "LeaveTypes");

            migrationBuilder.RenameTable(
                name: "LeaveTypes",
                newName: "leaveTypes");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "leaveTypes",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "leaveTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_leaveTypes",
                table: "leaveTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_leaveTypes_LeaveTypeId",
                table: "Leaves",
                column: "LeaveTypeId",
                principalTable: "leaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
