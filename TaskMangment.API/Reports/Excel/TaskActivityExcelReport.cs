using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class TaskActivityExcelReport
    {
        public static byte[] Build(
            List<TaskActivityReportDto> activities,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? roleTitle = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Task Activities");

            // RTL
            ws.RightToLeft = true;

            // Title
            ws.Cell(1, 1).Value = "تقرير عن حركات المهام";
            ws.Range(1, 1, 1, 5).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var reportDateRow = 2;
            if (fromDate.HasValue)
            {
                var toText = toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "-";
                var periodText = $"الفترة: من {fromDate:dd/MM/yyyy} إلى {toText}";
                if (!string.IsNullOrWhiteSpace(roleTitle))
                    periodText += $" | صلاحية: {roleTitle}";
                ws.Cell(2, 1).Value = periodText;
                ws.Range(2, 1, 2, 5).Merge().Style
                    .Font.SetFontSize(10)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                reportDateRow = 3;
            }

            // Report date
            ws.Cell(reportDateRow, 1).Value = $"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}";
            ws.Range(reportDateRow, 1, reportDateRow, 5).Merge().Style
                .Font.SetFontSize(10)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            // Headers
            var headerRow = reportDateRow + 1;
            ws.Cell(headerRow, 1).Value = "المهمة";
            ws.Cell(headerRow, 2).Value = "جهة التكليف";
            ws.Cell(headerRow, 3).Value = "التعليق";
            ws.Cell(headerRow, 4).Value = "تاريخ التعليق";
            ws.Cell(headerRow, 5).Value = "الموظف الذي علق";

            var headerRange = ws.Range(headerRow, 1, headerRow, 5);
            headerRange.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E5E5E5"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            // Data
            var row = headerRow + 1;
            for (int i = 0; i < activities.Count; i++)
            {
                var a = activities[i];

                ws.Cell(row, 1).Value = a.TaskTitleWithId;
                ws.Cell(row, 2).Value = a.AssignedBy;
                ws.Cell(row, 3).Value = a.Comment;
                ws.Cell(row, 4).Value = a.CommentDate.ToString("yyyy/MM/dd");
                ws.Cell(row, 5).Value = a.CommentedBy;

                var dataRange = ws.Range(row, 1, row, 5);
                dataRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                    .Alignment.SetWrapText(true);

                // Zebra
                if (i % 2 == 1)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                row++;
            }

            // Column widths
            ws.Column(1).Width = 35; // المهمة
            ws.Column(2).Width = 20; // جهة التكليف
            ws.Column(3).Width = 45; // التعليق
            ws.Column(4).Width = 15; // تاريخ التعليق
            ws.Column(5).Width = 22; // الموظف الذي علق

            // Freeze header
            ws.SheetView.FreezeRows(headerRow);

            // Print settings (اختياري)
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
