using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

public static class TaskDiscountAuditExcelReport
{
    public static byte[] Build(TaskDiscountAuditReportDto report, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Discount Audit");
        ws.RightToLeft = true;

        bool isOutgoing = report.MovementType == TaskMovementType.Outgoing;

        int totalCols = isOutgoing ? 8 : 7;
        // Columns:
        // 1 م
        // 2 المهمة
        // 3 جهة التكليف
        // 4 الموظف (Outgoing فقط)
        // next: تاريخ الإغلاق, الحالة, تلقائي, يدوي

        // ===== Header =====
        ws.Cell(1, 1).Value = "Task Manager System";
        ws.Range(1, 1, 1, totalCols).Merge().Style
            .Font.SetFontSize(9)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        ws.Cell(2, 1).Value = title;
        ws.Range(2, 1, 2, totalCols).Merge().Style
            .Font.SetBold()
            .Font.SetFontSize(18)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(3, 1).Value = $"الفترة: من {report.FromDate:yyyy/MM/dd} إلى {report.ToDate:yyyy/MM/dd}";
        ws.Range(3, 1, 3, totalCols).Merge().Style
            .Font.SetFontSize(10)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(4, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:yyyy/MM/dd}";
        ws.Range(4, 1, 4, totalCols).Merge().Style
            .Font.SetFontSize(9)
            .Font.SetFontColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // ===== Summary Boxes (MERGED to match table width totalCols) =====
        int cardsRow = 6;

        if (totalCols == 8)
        {
            // 2 + 2 + 2 + 2 = 8
            WriteCard(ws, cardsRow, 1, 2, "إجمالي الخصومات", report.GrandTotal.ToString("0.##"), "#FFD6D6");
            WriteCard(ws, cardsRow, 3, 4, "الخصومات التلقائية", report.GrandTotalAuto.ToString("0.##"), "#D6E4FF");
            WriteCard(ws, cardsRow, 5, 6, "الخصومات اليدوية", report.GrandTotalManual.ToString("0.##"), "#FFE6CC");
            WriteCard(ws, cardsRow, 7, 8, "عدد المهام", report.TotalTasks.ToString(), "#DFF5DF");
        }
        else
        {
            // totalCols == 7 => 2 + 2 + 2 + 1 = 7
            WriteCard(ws, cardsRow, 1, 2, "إجمالي الخصومات", report.GrandTotal.ToString("0.##"), "#FFD6D6");
            WriteCard(ws, cardsRow, 3, 4, "الخصومات التلقائية", report.GrandTotalAuto.ToString("0.##"), "#D6E4FF");
            WriteCard(ws, cardsRow, 5, 6, "الخصومات اليدوية", report.GrandTotalManual.ToString("0.##"), "#FFE6CC");
            WriteCard(ws, cardsRow, 7, 7, "عدد المهام", report.TotalTasks.ToString(), "#DFF5DF");
        }

        // spacing row
        int row = 9;

        // ===== Groups =====
        foreach (var group in report.Groups)
        {
            // Group title
            ws.Cell(row, 1).Value = $"الموظف: {group.EmployeeName}";
            ws.Range(row, 1, row, totalCols).Merge().Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#D6E4FF"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            row++;

            // Table header
            WriteHeader(ws, row, 1, "م");
            WriteHeader(ws, row, 2, "المهمة");
            WriteHeader(ws, row, 3, "جهة التكليف");

            int col = 4;
            if (isOutgoing)
            {
                WriteHeader(ws, row, col, "الموظف");
                col++;
            }

            WriteHeader(ws, row, col++, "تاريخ الإغلاق");
            WriteHeader(ws, row, col++, "الحالة");
            WriteHeader(ws, row, col++, "تلقائي");
            WriteHeader(ws, row, col++, "يدوي");

            ws.Range(row, 1, row, totalCols).Style
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E5E5E5"))
                .Font.SetBold()
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            row++;

            // Rows
            int i = 1;
            foreach (var task in group.Tasks)
            {
                int c = 1;
                ws.Cell(row, c++).Value = i++;

                ws.Cell(row, c++).Value = $"[{task.TaskId}] {task.Title}";
                ws.Cell(row, c++).Value = task.AssignedBy;

                if (isOutgoing)
                    ws.Cell(row, c++).Value = task.EmployeeName ?? "-";

                // ClosedDate
                if (task.ClosedDate.HasValue)
                {
                    ws.Cell(row, c).Value = task.ClosedDate.Value;
                    ws.Cell(row, c).Style.DateFormat.Format = "yyyy/MM/dd";
                }
                else
                {
                    ws.Cell(row, c).Value = "-";
                }
                c++;

                ws.Cell(row, c++).Value = task.Status;

                // AutoDiscount
                ws.Cell(row, c).Value = (double)task.AutoDiscount;
                ws.Cell(row, c).Style.NumberFormat.Format = "0.##";
                if (task.AutoDiscount > 0)
                    ws.Cell(row, c).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E8F0FF"));
                c++;

                // ManualDiscount
                ws.Cell(row, c).Value = (double)task.ManualDiscount;
                ws.Cell(row, c).Style.NumberFormat.Format = "0.##";
                if (task.ManualDiscount > 0)
                    ws.Cell(row, c).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FFF2D6"));
                c++;

                var dataRange = ws.Range(row, 1, row, totalCols);
                dataRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                    .Alignment.SetWrapText(true);

                if ((row % 2) == 0)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F7F7F7"));

                row++;
            }

            // Employee total line
            ws.Cell(row, 1).Value =
                $"إجمالي الموظف: {group.TotalDiscount} | تلقائي: {group.TotalAutoDiscount} | يدوي: {group.TotalManualDiscount} | عدد المهام: {group.TasksCount}";
            ws.Range(row, 1, row, totalCols).Merge().Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#FFF7CC"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            row += 2;
        }

        // ===== Column widths =====
        ws.Column(1).Width = 6;
        ws.Column(2).Width = 38;
        ws.Column(3).Width = 20;

        int startDynamic = 4;
        if (isOutgoing)
        {
            ws.Column(4).Width = 18;
            startDynamic = 5;
        }

        ws.Column(startDynamic + 0).Width = 16;
        ws.Column(startDynamic + 1).Width = 12;
        ws.Column(startDynamic + 2).Width = 12;
        ws.Column(startDynamic + 3).Width = 12;

        ws.SheetView.FreezeRows(8);

        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.FitToPages(1, 0);

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    // ✅ Merge title row and value row separately
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
        valueRng.Style.Font.SetBold().Font.SetFontSize(13);
    }

    private static void WriteHeader(IXLWorksheet ws, int row, int col, string text)
    {
        ws.Cell(row, col).Value = text;
        ws.Cell(row, col).Style
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Font.SetBold();
    }
}
