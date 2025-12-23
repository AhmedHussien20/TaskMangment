using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationAndEmailQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_PaymentVouchers_PaymentVoucherId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Tasks_WorkTaskId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_PaymentVoucherId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_WorkTaskId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "PaymentVoucherId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "WorkTaskId",
                table: "Attachments");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "EmailQueue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Cc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_EmailQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppQueue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppQueue", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_Status_ScheduledAt",
                table: "EmailQueue",
                columns: new[] { "Status", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Key",
                table: "EmailTemplates",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailQueue");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "WhatsAppQueue");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<int>(
                name: "PaymentVoucherId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkTaskId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PaymentVoucherId",
                table: "Attachments",
                column: "PaymentVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_WorkTaskId",
                table: "Attachments",
                column: "WorkTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_PaymentVouchers_PaymentVoucherId",
                table: "Attachments",
                column: "PaymentVoucherId",
                principalTable: "PaymentVouchers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Tasks_WorkTaskId",
                table: "Attachments",
                column: "WorkTaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
