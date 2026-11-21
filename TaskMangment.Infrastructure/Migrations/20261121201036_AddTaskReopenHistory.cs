using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskReopenHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskReopenHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    ReopenedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    ReopenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviousStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    PreviousClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviousCloseReason = table.Column<int>(type: "int", nullable: true),
                    PreviousDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReopenHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReopenHistories_Employees_ReopenedByEmployeeId",
                        column: x => x.ReopenedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskReopenHistories_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskReopenHistories_ReopenedByEmployeeId",
                table: "TaskReopenHistories",
                column: "ReopenedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReopenHistories_TaskId",
                table: "TaskReopenHistories",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskReopenHistories");
        }
    }
}
