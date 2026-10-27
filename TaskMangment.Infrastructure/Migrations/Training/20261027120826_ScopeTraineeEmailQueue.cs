using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations.Training
{
    /// <inheritdoc />
    public partial class ScopeTraineeEmailQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "training",
                table: "EmailQueue",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Existing rows: a personal email belongs to its recipient's copy; a holiday ForAll email belongs to
            // the copy that owns the holiday (ReferenceType 14 = OfficialHoliday). Anything left at 0 is never
            // sent as a ForAll email.
            migrationBuilder.Sql("""
                UPDATE q SET CompanyId = e.CompanyId
                FROM training.EmailQueue q
                JOIN training.Employees e ON e.Id = q.UserId;

                UPDATE q SET CompanyId = c.CompanyId
                FROM training.EmailQueue q
                JOIN training.CalendarEvents c ON c.Id = q.ReferenceId
                WHERE q.ForAll = 1 AND q.ReferenceType = 14 AND c.CompanyId IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueue_CompanyId",
                schema: "training",
                table: "EmailQueue",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailQueue_CompanyId",
                schema: "training",
                table: "EmailQueue");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "training",
                table: "EmailQueue");
        }
    }
}
