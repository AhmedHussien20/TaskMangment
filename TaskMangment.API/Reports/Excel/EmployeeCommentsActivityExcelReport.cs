using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class EmployeeCommentsActivityExcelReport
    {
        public static byte[] Build(
            List<EmployeeCommentsActivityReportDto> data,
            DateTime fromDate,
            DateTime? toDate,
            string? roleTitle = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Employee Activity");

            ws.RightToLeft = true;

            // ===== Summary =====
            int employeesCount = data.Count;
            int totalComments = data.Sum(x => x.TotalComments);
            var topEmployee = data.OrderByDescending(x => x.TotalComments).FirstOrDefault();
            var lastActiveDate = data.Max(x => x.LastCommentDate);

            // ===== Header =====
            ws.Cell(1, 1).Value = "Task Manager System";
            ws.Range(1, 1, 1, 6).Merge().Style
                .Font.SetFontSize(9)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            ws.Cell(2, 1).Value = "تقرير نشاط الموظفين والتفاعل مع المهام";
            ws.Range(2, 1, 2, 6).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var toText = toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "-";
            var periodText = $"الفترة: من {fromDate:dd/MM/yyyy} إلى {toText}";
            if (!string.IsNullOrWhiteSpace(roleTitle))
                periodText += $" | صلاحية: {roleTitle}";
            ws.Cell(3, 1).Value = periodText;
            ws.Range(3, 1, 3, 6).Merge().Style
                .Font.SetFontSize(10)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(4, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}";
            ws.Range(4, 1, 4, 6).Merge().Style
                .Font.SetFontSize(9)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== Summary Cards (MERGED per row to keep values) =====
            int cardsRow = 6;

            var topText = topEmployee != null
                ? $"{topEmployee.EmployeeName} ({topEmployee.TotalComments})"
                : "-";

            var lastText = lastActiveDate.HasValue
                ? lastActiveDate.Value.ToString("dd/MM/yyyy")
                : "-";

            // توزيع الأعمدة: 2 + 2 + 1 + 1 = 6 أعمدة
            WriteCard(ws, cardsRow, 1, 1, "عدد الموظفين", employeesCount.ToString(), "#D6E4FF");
            WriteCard(ws, cardsRow, 2, 2, "إجمالي التعليقات", totalComments.ToString(), "#DFF5DF");
            WriteCard(ws, cardsRow, 3, 4, "أكثر تفاعل", topText, "#FFE6CC");
            WriteCard(ws, cardsRow, 5, 6, "آخر نشاط", lastText, "#E6E6E6");

            // ===== Table Header =====
            int headerRow = 9;

            ws.Cell(headerRow, 1).Value = "ترتيب";
            ws.Cell(headerRow, 2).Value = "الموظف";
            ws.Cell(headerRow, 3).Value = "إجمالي التعليقات";
            ws.Cell(headerRow, 4).Value = "عدد المهام";
            ws.Cell(headerRow, 5).Value = "متوسط / مهمة";
            ws.Cell(headerRow, 6).Value = "آخر تعليق";

            var headerRange = ws.Range(headerRow, 1, headerRow, 6);
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

            foreach (var item in data.OrderByDescending(x => x.TotalComments))
            {
                ws.Cell(row, 1).Value = rank++;
                ws.Cell(row, 2).Value = item.EmployeeName;
                ws.Cell(row, 3).Value = item.TotalComments;
                ws.Cell(row, 4).Value = item.DistinctTasksCount;

                ws.Cell(row, 5).Value = (double)item.AvgCommentsPerTask;
                ws.Cell(row, 5).Style.NumberFormat.Format = "0.##";

                if (item.LastCommentDate.HasValue)
                {
                    ws.Cell(row, 6).Value = item.LastCommentDate.Value;
                    ws.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy";
                }
                else
                {
                    ws.Cell(row, 6).Value = "-";
                }

                var dataRange = ws.Range(row, 1, row, 6);
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
            ws.Column(1).Width = 8;   // ترتيب
            ws.Column(2).Width = 26;  // الموظف
            ws.Column(3).Width = 16;  // إجمالي التعليقات
            ws.Column(4).Width = 12;  // عدد المهام
            ws.Column(5).Width = 14;  // متوسط / مهمة
            ws.Column(6).Width = 14;  // آخر تعليق

            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, headerRow, 6).SetAutoFilter();

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
            // Title row merge
            var titleRng = ws.Range(row, colFrom, row, colTo);
            titleRng.Merge();
            titleRng.Value = title;

            // Value row merge
            var valueRng = ws.Range(row + 1, colFrom, row + 1, colTo);
            valueRng.Merge();
            valueRng.Value = value;

            // Whole card style (2 rows)
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
