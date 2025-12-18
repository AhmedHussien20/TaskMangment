using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_PaymentVouchers_VoucherId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_TaskComments_CommentId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Tasks_TaskId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "VoucherId",
                table: "Attachments",
                newName: "WorkTaskId");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "Attachments",
                newName: "TaskCommentId");

            migrationBuilder.RenameColumn(
                name: "CommentId",
                table: "Attachments",
                newName: "PaymentVoucherId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_VoucherId",
                table: "Attachments",
                newName: "IX_Attachments_WorkTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_TaskId",
                table: "Attachments",
                newName: "IX_Attachments_TaskCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_CommentId",
                table: "Attachments",
                newName: "IX_Attachments_PaymentVoucherId");

            migrationBuilder.AddColumn<int>(
    name: "AttachmentTypeId",
    table: "Attachments",
    type: "int",
    nullable: true);


            migrationBuilder.CreateTable(
                name: "AttachmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefId = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_AttachmentTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AttachmentTypeId",
                table: "Attachments",
                column: "AttachmentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_AttachmentTypes_AttachmentTypeId",
                table: "Attachments",
                column: "AttachmentTypeId",
                principalTable: "AttachmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_PaymentVouchers_PaymentVoucherId",
                table: "Attachments",
                column: "PaymentVoucherId",
                principalTable: "PaymentVouchers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_TaskComments_TaskCommentId",
                table: "Attachments",
                column: "TaskCommentId",
                principalTable: "TaskComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Tasks_WorkTaskId",
                table: "Attachments",
                column: "WorkTaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AttachmentTypes_AttachmentTypeId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_PaymentVouchers_PaymentVoucherId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_TaskComments_TaskCommentId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Tasks_WorkTaskId",
                table: "Attachments");

            migrationBuilder.DropTable(
                name: "AttachmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_AttachmentTypeId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "AttachmentTypeId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "WorkTaskId",
                table: "Attachments",
                newName: "VoucherId");

            migrationBuilder.RenameColumn(
                name: "TaskCommentId",
                table: "Attachments",
                newName: "TaskId");

            migrationBuilder.RenameColumn(
                name: "PaymentVoucherId",
                table: "Attachments",
                newName: "CommentId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_WorkTaskId",
                table: "Attachments",
                newName: "IX_Attachments_VoucherId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_TaskCommentId",
                table: "Attachments",
                newName: "IX_Attachments_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_PaymentVoucherId",
                table: "Attachments",
                newName: "IX_Attachments_CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_PaymentVouchers_VoucherId",
                table: "Attachments",
                column: "VoucherId",
                principalTable: "PaymentVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_TaskComments_CommentId",
                table: "Attachments",
                column: "CommentId",
                principalTable: "TaskComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Tasks_TaskId",
                table: "Attachments",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
