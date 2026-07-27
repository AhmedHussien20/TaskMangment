using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMangment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeTypesAndRoleScopeFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresBranchScope",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEmployeeTypeScope",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmployeeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SeesAllTypesInBranchScope = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTypes_Code",
                table: "EmployeeTypes",
                column: "Code",
                unique: true);

            // Seed canonical types before FKs (IDs match legacy FunctionCode enum values).
            migrationBuilder.Sql("""
                SET IDENTITY_INSERT EmployeeTypes ON;
                IF NOT EXISTS (SELECT 1 FROM EmployeeTypes WHERE Id = 1)
                  INSERT INTO EmployeeTypes (Id, Code, NameEn, NameAr, SeesAllTypesInBranchScope, CreatedDate, IsDeleted)
                  VALUES (1, N'Operations', N'Operations', N'عمليات', 1, SYSUTCDATETIME(), 0);
                IF NOT EXISTS (SELECT 1 FROM EmployeeTypes WHERE Id = 2)
                  INSERT INTO EmployeeTypes (Id, Code, NameEn, NameAr, SeesAllTypesInBranchScope, CreatedDate, IsDeleted)
                  VALUES (2, N'Accounting', N'Accounting', N'محاسبة', 0, SYSUTCDATETIME(), 0);
                IF NOT EXISTS (SELECT 1 FROM EmployeeTypes WHERE Id = 3)
                  INSERT INTO EmployeeTypes (Id, Code, NameEn, NameAr, SeesAllTypesInBranchScope, CreatedDate, IsDeleted)
                  VALUES (3, N'HR', N'HR', N'موارد بشرية', 0, SYSUTCDATETIME(), 0);
                IF NOT EXISTS (SELECT 1 FROM EmployeeTypes WHERE Id = 4)
                  INSERT INTO EmployeeTypes (Id, Code, NameEn, NameAr, SeesAllTypesInBranchScope, CreatedDate, IsDeleted)
                  VALUES (4, N'GeneralAffairs', N'General Affairs', N'شؤون عامة', 0, SYSUTCDATETIME(), 0);
                SET IDENTITY_INSERT EmployeeTypes OFF;

                UPDATE Employees SET FunctionCode = 1
                WHERE FunctionCode IS NULL OR FunctionCode NOT IN (SELECT Id FROM EmployeeTypes WHERE IsDeleted = 0);

                UPDATE EmployeeFunctionalScopes SET FunctionCode = 1
                WHERE FunctionCode NOT IN (SELECT Id FROM EmployeeTypes WHERE IsDeleted = 0);

                UPDATE Roles SET RequiresBranchScope = 1, RequiresEmployeeTypeScope = 1 WHERE Level >= 80;
                UPDATE Roles SET RequiresEmployeeTypeScope = 1 WHERE Level >= 60 AND Level < 80;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_FunctionCode",
                table: "Employees",
                column: "FunctionCode");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeFunctionalScopes_FunctionCode",
                table: "EmployeeFunctionalScopes",
                column: "FunctionCode");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeFunctionalScopes_EmployeeTypes_FunctionCode",
                table: "EmployeeFunctionalScopes",
                column: "FunctionCode",
                principalTable: "EmployeeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeTypes_FunctionCode",
                table: "Employees",
                column: "FunctionCode",
                principalTable: "EmployeeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeFunctionalScopes_EmployeeTypes_FunctionCode",
                table: "EmployeeFunctionalScopes");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeTypes_FunctionCode",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeeTypes");

            migrationBuilder.DropIndex(
                name: "IX_Employees_FunctionCode",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeFunctionalScopes_FunctionCode",
                table: "EmployeeFunctionalScopes");

            migrationBuilder.DropColumn(
                name: "RequiresBranchScope",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RequiresEmployeeTypeScope",
                table: "Roles");
        }
    }
}
