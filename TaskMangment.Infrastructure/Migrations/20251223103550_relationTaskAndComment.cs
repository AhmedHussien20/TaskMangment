using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class relationTaskAndComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCloseRequests_Tasks_WorkTaskId",
                table: "TaskCloseRequests");

            migrationBuilder.DropIndex(
                name: "IX_TaskCloseRequests_WorkTaskId",
                table: "TaskCloseRequests");

            migrationBuilder.DropColumn(
                name: "WorkTaskId",
                table: "TaskCloseRequests");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCloseRequests_TaskId",
                table: "TaskCloseRequests",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCloseRequests_Tasks_TaskId",
                table: "TaskCloseRequests",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCloseRequests_Tasks_TaskId",
                table: "TaskCloseRequests");

            migrationBuilder.DropIndex(
                name: "IX_TaskCloseRequests_TaskId",
                table: "TaskCloseRequests");

            migrationBuilder.AddColumn<int>(
                name: "WorkTaskId",
                table: "TaskCloseRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskCloseRequests_WorkTaskId",
                table: "TaskCloseRequests",
                column: "WorkTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCloseRequests_Tasks_WorkTaskId",
                table: "TaskCloseRequests",
                column: "WorkTaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
