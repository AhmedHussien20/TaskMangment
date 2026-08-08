using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class MostAssignedEmployeesExcelReport
    {
        public static byte[] Build(
            List<EmployeeAssignmentsReportDto> data,
            DateTime fromDate,
            DateTime? toDate,
            string? roleTitle = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Most Assigned");

            ws.RightToLeft = true;

            // ===== Summary =====
            int employeesCount = data.Count;
            int totalTasks = data.Sum(x => x.TotalTasks);
            var topEmployee = data.OrderByDescending(x => x.TotalTasks).FirstOrDefault();
            var leastEmployee = data.OrderBy(x => x.TotalTasks).FirstOrDefault();

            // ===== Header =====
            ws.Cell(1, 1).Value = "Task Manager System";
            ws.Range(1, 1, 1, 9).Merge().Style
                .Font.SetFontSize(9)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            ws.Cell(2, 1).Value = "تقرير توزيع المهام على الموظفين";
            ws.Range(2, 1, 2, 9).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var toText = toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "-";
            var periodText = $"الفترة: من {fromDate:dd/MM/yyyy} إلى {toText}";
            if (!string.IsNullOrWhiteSpace(roleTitle))
                periodText += $" | صلاحية: {roleTitle}";
            ws.Cell(3, 1).Value = periodText;
            ws.Range(3, 1, 3, 9).Merge().Style
                .Font.SetFontSize(10)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(4, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}";
            ws.Range(4, 1, 4, 9).Merge().Style
                .Font.SetFontSize(9)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== Summary Cards (MERGED to match table width 9 cols) =====
            int cardsRow = 6;

            var topText = topEmployee != null
                ? $"{topEmployee.EmployeeName} ({topEmployee.TotalTasks})"
                : "-";

            var leastText = leastEmployee != null
                ? $"{leastEmployee.EmployeeName} ({leastEmployee.TotalTasks})"
                : "-";

            // توزيع الأعمدة: 2 + 2 + 3 + 2 = 9 أعمدة
            WriteCard(ws, cardsRow, 1, 2, "عدد الموظفين", employeesCount.ToString(), "#D6E4FF");  // Blue
            WriteCard(ws, cardsRow, 3, 4, "إجمالي المهام", totalTasks.ToString(), "#DFF5DF");    // Green
            WriteCard(ws, cardsRow, 5, 7, "أعلى عبء عمل", topText, "#FFE6CC");                   // Orange (أوسع)
            WriteCard(ws, cardsRow, 8, 9, "أقل عبء عمل", leastText, "#E6E6E6");                  // Grey

            // ===== Table Header =====
            int headerRow = 9;

            ws.Cell(headerRow, 1).Value = "ترتيب";
            ws.Cell(headerRow, 2).Value = "الموظف";
            ws.Cell(headerRow, 3).Value = "الإجمالي";
            ws.Cell(headerRow, 4).Value = "جديدة";
            ws.Cell(headerRow, 5).Value = "قيد التنفيذ";
            ws.Cell(headerRow, 6).Value = "مغلقة";
            ws.Cell(headerRow, 7).Value = "متأخرة";
            ws.Cell(headerRow, 8).Value = "قريبة";
            ws.Cell(headerRow, 9).Value = "نسبة الإنجاز";

            var headerRange = ws.Range(headerRow, 1, headerRow, 9);
            headerRange.Style
                .Font.SetBold()
                .Font.SetFontSize(9)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E8F0FF"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            // ===== Data =====
            int row = headerRow + 1;
            int rank = 1;

            foreach (var item in data.OrderByDescending(x => x.TotalTasks))
            {
                ws.Cell(row, 1).Value = rank++;
                ws.Cell(row, 2).Value = item.EmployeeName;
                ws.Cell(row, 3).Value = item.TotalTasks;
                ws.Cell(row, 4).Value = item.NewTasks;
                ws.Cell(row, 5).Value = item.InProgressTasks;
                ws.Cell(row, 6).Value = item.ClosedTasks;
                ws.Cell(row, 7).Value = item.OverdueTasks;
                ws.Cell(row, 8).Value = item.ClosingSoonTasks;

                ws.Cell(row, 9).Value = (double)item.CompletionRate;
                ws.Cell(row, 9).Style.NumberFormat.Format = "0.##\" %\"";

                var dataRange = ws.Range(row, 1, row, 9);
                dataRange.Style
                    .Font.SetFontSize(9)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Alignment.SetWrapText(true);

                // Zebra
                if ((row - (headerRow + 1)) % 2 == 1)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                row++;
            }

            // ===== Layout =====
            ws.Column(1).Width = 8;
            ws.Column(2).Width = 26;
            ws.Column(3).Width = 12;
            ws.Column(4).Width = 10;
            ws.Column(5).Width = 12;
            ws.Column(6).Width = 10;
            ws.Column(7).Width = 10;
            ws.Column(8).Width = 10;
            ws.Column(9).Width = 14;

            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, headerRow, 9).SetAutoFilter();

            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        // ✅ Merge title row & value row separately (so value doesn't disappear)
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

            titleRng.Style.Font.SetBold().Font.SetFontSize(9);
            valueRng.Style.Font.SetBold().Font.SetFontSize(13);
        }
    }
}
