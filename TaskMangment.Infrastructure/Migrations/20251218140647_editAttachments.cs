using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
       name: "FK_Attachments_Tasks_WorkTaskId",
       table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_TaskComments_TaskCommentId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_PaymentVouchers_PaymentVoucherId",
                table: "Attachments");

            // Drop Indexes
            migrationBuilder.DropIndex(
                name: "IX_Attachments_WorkTaskId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_TaskCommentId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_PaymentVoucherId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "WorkTaskId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "TaskCommentId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "PaymentVoucherId",
                table: "Attachments");

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AttachmentTypes_AttachmentTypeId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_AttachmentTypeId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "AttachmentTypeId",
                table: "Attachments");

           
        }
    }
}
