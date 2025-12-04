using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSomeEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCloseRequests_Tasks_TaskId",
                table: "TaskCloseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskExtensionRequests_Tasks_TaskId",
                table: "TaskExtensionRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "CourseSubjects");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskExtensionRequests",
                newName: "WorkTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskExtensionRequests_TaskId",
                table: "TaskExtensionRequests",
                newName: "IX_TaskExtensionRequests_WorkTaskId");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskCloseRequests",
                newName: "WorkTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskCloseRequests_TaskId",
                table: "TaskCloseRequests",
                newName: "IX_TaskCloseRequests_WorkTaskId");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaskAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCloseRequests_Tasks_WorkTaskId",
                table: "TaskCloseRequests",
                column: "WorkTaskId",
                principalTable: "Tasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskExtensionRequests_Tasks_WorkTaskId",
                table: "TaskExtensionRequests",
                column: "WorkTaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskCloseRequests_Tasks_WorkTaskId",
                table: "TaskCloseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskExtensionRequests_Tasks_WorkTaskId",
                table: "TaskExtensionRequests");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaskAssignments");

            migrationBuilder.RenameColumn(
                name: "WorkTaskId",
                table: "TaskExtensionRequests",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskExtensionRequests_WorkTaskId",
                table: "TaskExtensionRequests",
                newName: "IX_TaskExtensionRequests_TaskId");

            migrationBuilder.RenameColumn(
                name: "WorkTaskId",
                table: "TaskCloseRequests",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskCloseRequests_WorkTaskId",
                table: "TaskCloseRequests",
                newName: "IX_TaskCloseRequests_TaskId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "CourseSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskCloseRequests_Tasks_TaskId",
                table: "TaskCloseRequests",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskExtensionRequests_Tasks_TaskId",
                table: "TaskExtensionRequests",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
