using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "EmailQueue",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_UserId",
                table: "EmailQueue",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailQueue_UserId",
                table: "EmailQueue");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmailQueue");
        }
    }
}
