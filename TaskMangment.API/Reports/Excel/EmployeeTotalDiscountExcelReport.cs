using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class EmployeeTotalDiscountExcelReport
    {
        public static byte[] Build(
            List<EmployeeTotalDiscountReportRowDto> data,
            DateTime? fromDate,
            DateTime? toDate,
            string? roleTitle)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Employee Discounts");
            ws.RightToLeft = true;

            var title = BuildTitle(fromDate, toDate, roleTitle);

            ws.Cell(1, 1).Value = "Task Manager System";
            ws.Range(1, 1, 1, 4).Merge().Style
                .Font.SetFontSize(9)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            ws.Cell(2, 1).Value = title;
            ws.Range(2, 1, 2, 4).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(3, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:yyyy/MM/dd}";
            ws.Range(3, 1, 3, 4).Merge().Style
                .Font.SetFontSize(9)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(5, 1).Value = "إجمالي الخصومات";
            ws.Cell(5, 2).Value = (double)data.Sum(x => x.TotalDiscount);
            ws.Cell(5, 2).Style.NumberFormat.Format = "0.##";
            ws.Range(5, 1, 5, 2).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#FFD6D6"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            const int headerRow = 7;
            ws.Cell(headerRow, 1).Value = "م";
            ws.Cell(headerRow, 2).Value = "الموظف";
            ws.Cell(headerRow, 3).Value = "الصلاحية";
            ws.Cell(headerRow, 4).Value = "إجمالي الخصومات";

            ws.Range(headerRow, 1, headerRow, 4).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E8F0FF"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            var row = headerRow + 1;
            var index = 1;

            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = index++;
                ws.Cell(row, 2).Value = item.EmployeeName;
                ws.Cell(row, 3).Value = item.RoleTitle;
                ws.Cell(row, 4).Value = (double)item.TotalDiscount;
                ws.Cell(row, 4).Style.NumberFormat.Format = "0.##";

                var range = ws.Range(row, 1, row, 4);
                range.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                if (row % 2 == 0)
                    range.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F7F7F7"));

                row++;
            }

            ws.Column(1).Width = 8;
            ws.Column(2).Width = 28;
            ws.Column(3).Width = 24;
            ws.Column(4).Width = 18;

            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, headerRow, 4).SetAutoFilter();
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        private static string BuildTitle(DateTime? fromDate, DateTime? toDate, string? roleTitle)
        {
            var title = fromDate.HasValue
                ? $"اجمالي الخصومات من {fromDate.Value:yyyy/MM/dd} الي {(toDate ?? DateTime.Now):yyyy/MM/dd}"
                : "تقرير اجمالي الخصومات من البداية";

            if (!string.IsNullOrWhiteSpace(roleTitle))
                title += $" ل صلاحية {roleTitle}";

            return title;
        }
    }
}
