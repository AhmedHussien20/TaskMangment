using ClosedXML.Excel;
using TaskMangment.Application.DTOs;

namespace TaskMangment.API.Reports.Excel
{
    public static class EmployeesExcelReport
    {
        public static byte[] Build(List<EmployeeGetDto> employees)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Employees");

            ws.RightToLeft = true;

            ws.Cell(1, 1).Value = "قائمة الموظفين";
            ws.Range(1, 1, 1, 7).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(2, 1).Value = $"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}";
            ws.Range(2, 1, 2, 7).Merge().Style
                .Font.SetFontSize(10)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            int headerRow = 4;
            ws.Cell(headerRow, 1).Value = "الكود";
            ws.Cell(headerRow, 2).Value = "الاسم";
            ws.Cell(headerRow, 3).Value = "الفرع";
            ws.Cell(headerRow, 4).Value = "البريد الإلكتروني";
            ws.Cell(headerRow, 5).Value = "الجوال";
            ws.Cell(headerRow, 6).Value = "الأدوار";
            ws.Cell(headerRow, 7).Value = "آخر تسجيل دخول";

            var headerRange = ws.Range(headerRow, 1, headerRow, 7);
            headerRange.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E5E5E5"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            int row = headerRow + 1;
            for (int i = 0; i < employees.Count; i++)
            {
                var emp = employees[i];

                ws.Cell(row, 1).Value = emp.Id;
                ws.Cell(row, 2).Value = emp.FullName;
                ws.Cell(row, 3).Value = emp.BranchName ?? "-";
                ws.Cell(row, 4).Value = emp.Email ?? "-";
                ws.Cell(row, 5).Value = emp.Mobile ?? "-";
                ws.Cell(row, 6).Value = emp.Roles != null && emp.Roles.Any()
                    ? string.Join("، ", emp.Roles)
                    : "-";
                ws.Cell(row, 7).Value = emp.LastLoginDate.HasValue
                    ? emp.LastLoginDate.Value.ToString("yyyy/MM/dd HH:mm")
                    : "-";

                var dataRange = ws.Range(row, 1, row, 7);
                dataRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                    .Alignment.SetWrapText(true);

                if (i % 2 == 1)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                row++;
            }

            ws.Column(1).Width = 8;
            ws.Column(2).Width = 28;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 28;
            ws.Column(5).Width = 16;
            ws.Column(6).Width = 24;
            ws.Column(7).Width = 20;

            ws.SheetView.FreezeRows(headerRow);
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
