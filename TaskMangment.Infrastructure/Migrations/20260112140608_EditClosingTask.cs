using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditClosingTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CloseReason",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosedByUserId",
                table: "Tasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseReason",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "Tasks");
        }
    }
}
