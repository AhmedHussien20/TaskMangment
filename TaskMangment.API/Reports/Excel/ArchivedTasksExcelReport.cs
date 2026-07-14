using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class ArchivedTasksExcelReport
    {
        public static byte[] Build(
            List<EmployeeArchivedTasksReportDto> data,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? roleTitle = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Archived Tasks");

            ws.RightToLeft = true;

            // ===== Metrics =====
            int totalEmployees = data.Count;
            int totalTasks = data.Sum(x => x.TotalTasks);
            int totalArchived = data.Sum(x => x.ArchivedTasksCount);
            decimal maxArchiveRate = data.Any() ? data.Max(x => x.ArchiveRate) : 0;

            // ===== Header =====
            ws.Cell(1, 1).Value = "Task Manager System";
            ws.Range(1, 1, 1, 5).Merge().Style
                .Font.SetFontSize(9)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            ws.Cell(2, 1).Value = "تقرير تحليل أرشفة المهام حسب الموظفين";
            ws.Range(2, 1, 2, 5).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var headerRow = 3;
            if (fromDate.HasValue)
            {
                var toText = toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "-";
                var periodText = $"الفترة: من {fromDate:dd/MM/yyyy} إلى {toText}";
                if (!string.IsNullOrWhiteSpace(roleTitle))
                    periodText += $" | صلاحية: {roleTitle}";
                ws.Cell(3, 1).Value = periodText;
                ws.Range(3, 1, 3, 5).Merge().Style
                    .Font.SetFontSize(10)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRow = 4;
            }

            ws.Cell(headerRow, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}";
            ws.Range(headerRow, 1, headerRow, 5).Merge().Style
                .Font.SetFontSize(10)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== Summary Cards (match table width 5 cols) =====
            int cardsRow = 5;

            // توزيع الأعمدة: 2 + 1 + 1 + 1 = 5 أعمدة
            WriteCard(ws, cardsRow, 1, 1, "عدد الموظفين", totalEmployees.ToString(), "#D6E4FF");              // Blue Light
            WriteCard(ws, cardsRow, 2, 2, "إجمالي المهام", totalTasks.ToString(), "#E6E6E6");                 // Grey Light
            WriteCard(ws, cardsRow, 3, 3, "إجمالي المؤرشف", totalArchived.ToString(), "#FFE6CC");            // Orange Light
            WriteCard(ws, cardsRow, 4, 5, "أعلى نسبة أرشفة", $"{maxArchiveRate:0.##}%", "#FFD6D6");          // Red Light

            // ===== Table Header =====
            int tableHeaderRow = 8;

            ws.Cell(tableHeaderRow, 1).Value = "ترتيب";
            ws.Cell(tableHeaderRow, 2).Value = "اسم الموظف";
            ws.Cell(tableHeaderRow, 3).Value = "إجمالي المهام";
            ws.Cell(tableHeaderRow, 4).Value = "المؤرشفة";
            ws.Cell(tableHeaderRow, 5).Value = "نسبة الأرشفة %";

            var headerRange = ws.Range(tableHeaderRow, 1, tableHeaderRow, 5);
            headerRange.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E5E5E5"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            // ===== Data =====
            int row = tableHeaderRow + 1;
            int index = 1;

            foreach (var item in data.OrderByDescending(x => x.ArchiveRate))
            {
                ws.Cell(row, 1).Value = index;
                ws.Cell(row, 2).Value = item.EmployeeName;
                ws.Cell(row, 3).Value = item.TotalTasks;
                ws.Cell(row, 4).Value = item.ArchivedTasksCount;

                ws.Cell(row, 5).Value = (double)item.ArchiveRate;
                ws.Cell(row, 5).Style.NumberFormat.Format = "0.##\"%\"";

                var dataRange = ws.Range(row, 1, row, 5);
                dataRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                // Zebra
                if (index % 2 == 0)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                // Rate color rules (same as PDF)
                var rateColor =
                    item.ArchiveRate > 50 ? "#FFD6D6" :
                    item.ArchiveRate >= 20 ? "#FFE6CC" :
                    "#DFF5DF";

                ws.Cell(row, 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml(rateColor));

                index++;
                row++;
            }

            // ===== Legend =====
            ws.Cell(row + 1, 1).Value = "🟥 نسبة عالية   🟧 متوسطة   🟩 طبيعية";
            ws.Range(row + 1, 1, row + 1, 5).Merge().Style
                .Font.SetFontSize(9)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            // ===== Layout =====
            ws.Column(1).Width = 10; // ترتيب
            ws.Column(2).Width = 28; // اسم الموظف
            ws.Column(3).Width = 14; // إجمالي المهام
            ws.Column(4).Width = 12; // المؤرشفة
            ws.Column(5).Width = 16; // نسبة الأرشفة

            ws.SheetView.FreezeRows(headerRow);
            ws.RangeUsed().Style.Alignment.WrapText = true;
            ws.Range(headerRow, 1, headerRow, 5).SetAutoFilter();

            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        // ✅ Merge title row and value row separately (so value doesn't disappear)
        private static void WriteCard(
            IXLWorksheet ws,
            int row,
            int colFrom,
            int colTo,
            string title,
            string value,
            string bgHex)
        {
            // Title row
            var titleRng = ws.Range(row, colFrom, row, colTo);
            titleRng.Merge();
            titleRng.Value = title;

            // Value row
            var valueRng = ws.Range(row + 1, colFrom, row + 1, colTo);
            valueRng.Merge();
            valueRng.Value = value;

            // Whole card style
            var cardRng = ws.Range(row, colFrom, row + 1, colTo);
            cardRng.Style
                .Fill.SetBackgroundColor(XLColor.FromHtml(bgHex))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            titleRng.Style.Font.SetBold().Font.SetFontSize(10);
            valueRng.Style.Font.SetBold().Font.SetFontSize(16);
        }
    }
}
