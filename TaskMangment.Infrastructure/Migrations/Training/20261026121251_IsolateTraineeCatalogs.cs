using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations.Training
{
    /// <inheritdoc />
    public partial class IsolateTraineeCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeTypes_Code",
                schema: "training",
                table: "EmployeeTypes");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "training",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "training",
                table: "LeaveTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "training",
                table: "EmployeeTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "training",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Existing rows all start as templates (CompanyId 0). Before the per-company unique
            // indexes exist: hand rows a trainee created back to that trainee, give every existing
            // copy its own catalog, and point its data at that copy instead of the template.
            migrationBuilder.Sql("""
                -- 1. Rows a trainee created (not one of the catalog rows copied from dbo) belong to that trainee.
                UPDATE et SET CompanyId = e.CompanyId
                FROM training.EmployeeTypes et
                JOIN training.Employees e ON e.Id = et.CreatedBy
                WHERE NOT EXISTS (SELECT 1 FROM dbo.EmployeeTypes d WHERE d.Id = et.Id AND d.Code = et.Code);

                UPDATE p SET CompanyId = e.CompanyId
                FROM training.Permissions p
                JOIN training.Employees e ON e.Id = p.CreatedBy
                WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions d WHERE d.Id = p.Id AND d.Code = p.Code);

                UPDATE lt SET CompanyId = e.CompanyId
                FROM training.LeaveTypes lt
                JOIN training.Employees e ON e.Id = lt.CreatedBy
                WHERE NOT EXISTS (SELECT 1 FROM dbo.LeaveTypes d WHERE d.Id = lt.Id AND d.NameEn = lt.NameEn);

                UPDATE a SET CompanyId = e.CompanyId
                FROM training.AuditLogs a
                JOIN training.Employees e ON e.Id = TRY_CAST(a.ChangedBy AS int);

                -- 2. Every existing copy gets its own rows from the templates.
                INSERT INTO training.EmployeeTypes (Code, NameEn, NameAr, SeesAllTypesInBranchScope, CompanyId, CreatedBy, CreatedDate, IsDeleted)
                SELECT t.Code, t.NameEn, t.NameAr, t.SeesAllTypesInBranchScope, c.Id, t.CreatedBy, SYSUTCDATETIME(), 0
                FROM training.EmployeeTypes t
                CROSS JOIN training.Companies c
                WHERE t.CompanyId = 0 AND t.IsDeleted = 0
                  AND NOT EXISTS (SELECT 1 FROM training.EmployeeTypes x WHERE x.CompanyId = c.Id AND x.Code = t.Code);

                INSERT INTO training.Permissions (Code, Name, Description, CompanyId, CreatedBy, CreatedDate, IsDeleted)
                SELECT t.Code, t.Name, t.Description, c.Id, t.CreatedBy, SYSUTCDATETIME(), 0
                FROM training.Permissions t
                CROSS JOIN training.Companies c
                WHERE t.CompanyId = 0 AND t.IsDeleted = 0
                  AND NOT EXISTS (SELECT 1 FROM training.Permissions x WHERE x.CompanyId = c.Id AND x.Code = t.Code);

                INSERT INTO training.LeaveTypes (NameAr, NameEn, IsPaid, MaxDaysPerYear, CompanyId, CreatedBy, CreatedDate, IsDeleted)
                SELECT t.NameAr, t.NameEn, t.IsPaid, t.MaxDaysPerYear, c.Id, t.CreatedBy, SYSUTCDATETIME(), 0
                FROM training.LeaveTypes t
                CROSS JOIN training.Companies c
                WHERE t.CompanyId = 0 AND t.IsDeleted = 0
                  AND NOT EXISTS (SELECT 1 FROM training.LeaveTypes x WHERE x.CompanyId = c.Id AND x.NameEn = t.NameEn);

                -- 3. Repoint each copy's data from the template rows to its own rows.
                UPDATE e SET FunctionCode = cp.Id
                FROM training.Employees e
                JOIN training.EmployeeTypes t ON t.Id = e.FunctionCode AND t.CompanyId = 0
                JOIN training.EmployeeTypes cp ON cp.CompanyId = e.CompanyId AND cp.Code = t.Code;

                UPDATE s SET FunctionCode = cp.Id
                FROM training.EmployeeFunctionalScopes s
                JOIN training.Employees e ON e.Id = s.EmployeeId
                JOIN training.EmployeeTypes t ON t.Id = s.FunctionCode AND t.CompanyId = 0
                JOIN training.EmployeeTypes cp ON cp.CompanyId = e.CompanyId AND cp.Code = t.Code;

                UPDATE r SET EmployeeTypeId = cp.Id
                FROM training.Roles r
                JOIN training.EmployeeTypes t ON t.Id = r.EmployeeTypeId AND t.CompanyId = 0
                JOIN training.EmployeeTypes cp ON cp.CompanyId = r.CompanyId AND cp.Code = t.Code;

                UPDATE rp SET PermissionId = cp.Id
                FROM training.RolePermission rp
                JOIN training.Roles r ON r.Id = rp.RoleId
                JOIN training.Permissions t ON t.Id = rp.PermissionId AND t.CompanyId = 0
                JOIN training.Permissions cp ON cp.CompanyId = r.CompanyId AND cp.Code = t.Code;

                UPDATE l SET LeaveTypeId = cp.Id
                FROM training.Leaves l
                JOIN training.LeaveTypes t ON t.Id = l.LeaveTypeId AND t.CompanyId = 0
                JOIN training.LeaveTypes cp ON cp.CompanyId = l.CompanyId AND cp.NameEn = t.NameEn;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_CompanyId_Code",
                schema: "training",
                table: "Permissions",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_CompanyId",
                schema: "training",
                table: "LeaveTypes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_CompanyId_Code",
                schema: "training",
                table: "EmployeeTypes",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CompanyId",
                schema: "training",
                table: "AuditLogs",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_CompanyId_Code",
                schema: "training",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_LeaveTypes_CompanyId",
                schema: "training",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTypes_CompanyId_Code",
                schema: "training",
                table: "EmployeeTypes");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CompanyId",
                schema: "training",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "training",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "training",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "training",
                table: "EmployeeTypes");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "training",
                table: "AuditLogs");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_Code",
                schema: "training",
                table: "EmployeeTypes",
                column: "Code",
                unique: true);
        }
    }
}
