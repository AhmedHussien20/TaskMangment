using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class BranchTasksExcelReport
    {
        public static byte[] Build(
            List<BranchTaskReportRowDto> data,
            string branchName,
            DateTime fromDate,
            DateTime? toDate)
        {
            var effectiveToDate = toDate ?? DateTime.Now;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Branch Tasks");

            ws.RightToLeft = true;

            // ===== Header =====
            ws.Cell(1, 1).Value = "Task Manager System";
            ws.Range(1, 1, 1, 8).Merge().Style
                .Font.SetFontSize(9)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            ws.Cell(2, 1).Value = "تقرير مهام الفرع";
            ws.Range(2, 1, 2, 8).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(3, 1).Value = $"الفرع: {branchName}";
            ws.Range(3, 1, 3, 8).Merge().Style
                .Font.SetFontSize(10)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(4, 1).Value = $"الفترة: من {fromDate:dd/MM/yyyy} إلى {effectiveToDate:dd/MM/yyyy}";
            ws.Range(4, 1, 4, 8).Merge().Style
                .Font.SetFontSize(10)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(5, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}";
            ws.Range(5, 1, 5, 8).Merge().Style
                .Font.SetFontSize(9)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== Table Header =====
            int headerRow = 7;

            ws.Cell(headerRow, 1).Value = "ترتيب";
            ws.Cell(headerRow, 2).Value = "المهمة";
            ws.Cell(headerRow, 3).Value = "جهة التكليف";
            ws.Cell(headerRow, 4).Value = "الموظفين";
            ws.Cell(headerRow, 5).Value = "الحالة";
            ws.Cell(headerRow, 6).Value = "تاريخ الإنشاء";
            ws.Cell(headerRow, 7).Value = "تاريخ الانتهاء";
            ws.Cell(headerRow, 8).Value = "عدد طلبات التمديد";

            var headerRange = ws.Range(headerRow, 1, headerRow, 8);
            headerRange.Style
                .Font.SetBold()
                .Font.SetFontSize(9)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E8F0FF"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Alignment.SetWrapText(true);

            // ===== Data =====
            int row = headerRow + 1;
            int rank = 1;

            foreach (var item in data
                         .OrderBy(x => x.Title)
                         .ThenBy(x => x.CreatedDate)
                         .ThenBy(x => x.TaskId))
            {
                ws.Cell(row, 1).Value = rank++;
                ws.Cell(row, 2).Value = $"[{item.TaskId}] {item.Title}";
                ws.Cell(row, 3).Value = item.AssignedBy;

                var employeesText = (item.Employees == null || item.Employees.Count == 0)
                    ? "-"
                    : string.Join("، ", item.Employees.Select(e => e.Name));

                ws.Cell(row, 4).Value = employeesText;

                ws.Cell(row, 5).Value = item.StatusText;

                ws.Cell(row, 6).Value = item.CreatedDate;
                ws.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy";

                ws.Cell(row, 7).Value = item.EffectiveDueDate;
                ws.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy";

                ws.Cell(row, 8).Value = item.ExtensionRequestsCount;

                var dataRange = ws.Range(row, 1, row, 8);
                dataRange.Style
                    .Font.SetFontSize(9)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Alignment.SetWrapText(true);

                ws.Row(row).Height = 22;

                if ((row - (headerRow + 1)) % 2 == 1)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                row++;
            }

            // ===== Layout =====
            ws.Column(1).Width = 8;
            ws.Column(2).Width = 45;
            ws.Column(3).Width = 22;
            ws.Column(4).Width = 55;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 14;
            ws.Column(7).Width = 14;
            ws.Column(8).Width = 18;

            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, headerRow, 8).SetAutoFilter();

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
