using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class TaskMovementExcelReport
    {
        public static byte[] Build(List<TaskMovementReportDto> items, TaskMovementType movementType)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Task Movements");

            ws.RightToLeft = true;

            // ===== Title =====
            var title = movementType == TaskMovementType.Incoming
                ? "تقرير حركة المهام الواردة لليوم"
                : "تقرير حركة المهام الصادرة لليوم";

            ws.Cell(1, 1).Value = title;
            ws.Range(1, 1, 1, 5).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            // ===== SubTitle (ReportTitle) =====
            var subTitle = items.FirstOrDefault()?.ReportTitle;
            if (!string.IsNullOrWhiteSpace(subTitle))
            {
                ws.Cell(2, 1).Value = subTitle;
                ws.Range(2, 1, 2, 5).Merge().Style
                    .Font.SetFontSize(13)
                    .Font.SetFontColor(XLColor.FromHtml("#555555"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            }

            // ===== Date =====
            ws.Cell(3, 1).Value = $"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}";
            ws.Range(3, 1, 3, 5).Merge().Style
                .Font.SetFontSize(10)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            // ===== Header =====
            const int headerRow = 5;
            ws.Cell(headerRow, 1).Value = "المهمة";
            ws.Cell(headerRow, 2).Value = "جهة التكليف";
            ws.Cell(headerRow, 3).Value = "التعليق";
            ws.Cell(headerRow, 4).Value = "تاريخ التعليق";
            ws.Cell(headerRow, 5).Value = "الموظف الذي علّق";

            var headerRange = ws.Range(headerRow, 1, headerRow, 5);
            headerRange.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E5E5E5"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            // ===== Data =====
            var row = headerRow + 1;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                ws.Cell(row, 1).Value = item.TaskTitleWithId;
                ws.Cell(row, 2).Value = item.AssignedBy;
                ws.Cell(row, 3).Value = item.CommentText;
                ws.Cell(row, 4).Value = item.CommentDate.ToString("yyyy/MM/dd HH:mm");
                ws.Cell(row, 5).Value = item.CommentedBy;

                var dataRange = ws.Range(row, 1, row, 5);
                dataRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                    .Alignment.SetWrapText(true);

                // Zebra rows
                if (i % 2 == 1)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                row++;
            }

            // ===== Column widths =====
            ws.Column(1).Width = 35;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 45;
            ws.Column(4).Width = 18;
            ws.Column(5).Width = 22;

            // Freeze header
            ws.SheetView.FreezeRows(headerRow);

            // Print (اختياري)
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
