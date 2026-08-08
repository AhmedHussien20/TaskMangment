using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editoffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Offers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offers_BranchId",
                table: "Offers",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Branches_BranchId",
                table: "Offers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Branches_BranchId",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Offers_BranchId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Offers");
        }
    }
}
