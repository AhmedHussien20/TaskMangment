using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentReferenceLookupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Attachments_ReferenceId_AttachmentType_IsDeleted'
      AND object_id = OBJECT_ID(N'Attachments'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Attachments_ReferenceId_AttachmentType_IsDeleted]
    ON [Attachments] ([ReferenceId], [AttachmentType], [IsDeleted])
    INCLUDE ([FilePath]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Attachments_ReferenceId_AttachmentType_IsDeleted'
      AND object_id = OBJECT_ID(N'Attachments'))
BEGIN
    DROP INDEX [IX_Attachments_ReferenceId_AttachmentType_IsDeleted] ON [Attachments];
END
");
        }
    }
}
