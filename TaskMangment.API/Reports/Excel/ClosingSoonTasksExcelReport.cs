using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public static class ClosingSoonTasksExcelReport
    {
        public static byte[] Build(List<TasksClosingSoonDto> tasks)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Closing Soon");

            ws.RightToLeft = true;

            // ===== Summary =====
            int totalTasks = tasks.Count;
            int affectedEmployees = tasks.Select(t => t.EmployeeName).Distinct().Count();
            int affectedBranches = tasks.Select(t => t.BranchName).Distinct().Count();

            const int totalCols = 9;

            // ===== Header =====
            ws.Cell(1, 1).Value = "Task Manager System";
            ws.Range(1, 1, 1, totalCols).Merge().Style
                .Font.SetFontSize(9)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            ws.Cell(2, 1).Value = "تقرير المهام القريبة من الإغلاق";
            ws.Range(2, 1, 2, totalCols).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(3, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}";
            ws.Range(3, 1, 3, totalCols).Merge().Style
                .Font.SetFontSize(9)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ===== Summary Cards (3 cards => 3 + 3 + 3 = 9) =====
            int cardsRow = 5;

            WriteCard(ws, cardsRow, 1, 3, "عدد المهام القريبة من الإغلاق", totalTasks.ToString(), "#D6E4FF");  // blue
            WriteCard(ws, cardsRow, 4, 6, "الموظفون المتأثرون", affectedEmployees.ToString(), "#DFF5DF");      // green
            WriteCard(ws, cardsRow, 7, 9, "الفروع المتأثرة", affectedBranches.ToString(), "#E6E6E6");         // grey

            // ===== Table Header =====
            int headerRow = 8;

            ws.Cell(headerRow, 1).Value = "رقم";
            ws.Cell(headerRow, 2).Value = "عنوان المهمة";
            ws.Cell(headerRow, 3).Value = "الموظف";
            ws.Cell(headerRow, 4).Value = "الشركة";
            ws.Cell(headerRow, 5).Value = "الفرع";
            ws.Cell(headerRow, 6).Value = "المنطقة";
            ws.Cell(headerRow, 7).Value = "جهة التكليف";
            ws.Cell(headerRow, 8).Value = "تاريخ الإغلاق المتوقع";
            ws.Cell(headerRow, 9).Value = "الحالة";

            var headerRange = ws.Range(headerRow, 1, headerRow, totalCols);
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
            int index = 1;

            foreach (var t in tasks.OrderBy(x => x.ClosedDate)) // نفس PDF
            {
                ws.Cell(row, 1).Value = index++;
                ws.Cell(row, 2).Value = t.Title;
                ws.Cell(row, 3).Value = t.EmployeeName;
                ws.Cell(row, 4).Value = t.CompanyName;
                ws.Cell(row, 5).Value = t.BranchName;
                ws.Cell(row, 6).Value = t.AreaName;
                ws.Cell(row, 7).Value = t.AssignedBy;

                if (t.DueDate.HasValue)
                {
                    ws.Cell(row, 8).Value = t.DueDate.Value;
                    ws.Cell(row, 8).Style.DateFormat.Format = "dd/MM/yyyy";
                }
                else
                {
                    ws.Cell(row, 8).Value = "-";
                }

                ws.Cell(row, 9).Value = t.Status.ToString();

                var dataRange = ws.Range(row, 1, row, totalCols);
                dataRange.Style
                    .Font.SetFontSize(9)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                    .Alignment.SetWrapText(true);

                if ((row - (headerRow + 1)) % 2 == 1)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                row++;
            }

            // ===== Column widths (practical) =====
            ws.Column(1).Width = 6;
            ws.Column(2).Width = 30;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 18;
            ws.Column(5).Width = 16;
            ws.Column(6).Width = 14;
            ws.Column(7).Width = 18;
            ws.Column(8).Width = 18;
            ws.Column(9).Width = 12;

            // Freeze + Filter
            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, headerRow, totalCols).SetAutoFilter();

            // Print (اختياري)
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }

        // ✅ merged card that keeps both title and value visible
        private static void WriteCard(
            IXLWorksheet ws,
            int row,
            int colFrom,
            int colTo,
            string title,
            string value,
            string bgHex)
        {
            var titleRng = ws.Range(row, colFrom, row, colTo);
            titleRng.Merge();
            titleRng.Value = title;

            var valueRng = ws.Range(row + 1, colFrom, row + 1, colTo);
            valueRng.Merge();
            valueRng.Value = value;

            var cardRng = ws.Range(row, colFrom, row + 1, colTo);
            cardRng.Style
                .Fill.SetBackgroundColor(XLColor.FromHtml(bgHex))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            titleRng.Style.Font.SetBold().Font.SetFontSize(9);
            valueRng.Style.Font.SetBold().Font.SetFontSize(16);
        }
    }
}
