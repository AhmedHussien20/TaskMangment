using ClosedXML.Excel;
using TaskMangment.Application.ReportDTOs;

public static class TasksExcelReport
{
    public static byte[] Build(List<TaskReportDto> tasks)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Tasks");

        ws.RightToLeft = true;

        // ===== Header =====
        ws.Cell(1, 1).Value = "Task Manager System";
        ws.Range(1, 1, 1, 5).Merge().Style
            .Font.SetFontSize(9)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        ws.Cell(2, 1).Value = "تقرير المهام";
        ws.Range(2, 1, 2, 5).Merge().Style
            .Font.SetBold()
            .Font.SetFontSize(20)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(3, 1).Value = $"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}";
        ws.Range(3, 1, 3, 5).Merge().Style
            .Font.SetFontSize(10)
            .Font.SetFontColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // ===== Table Header =====
        int headerRow = 5;

        ws.Cell(headerRow, 1).Value = "عنوان المهمة";
        ws.Cell(headerRow, 2).Value = "الحالة";
        ws.Cell(headerRow, 3).Value = "الأولوية";
        ws.Cell(headerRow, 4).Value = "المسؤول";
        ws.Cell(headerRow, 5).Value = "تاريخ الاستحقاق";

        var headerRange = ws.Range(headerRow, 1, headerRow, 5);
        headerRange.Style
            .Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.FromHtml("#E5E5E5"))
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetInsideBorder(XLBorderStyleValues.Thin)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // ===== Data =====
        int row = headerRow + 1;

        foreach (var t in tasks)
        {
            ws.Cell(row, 1).Value = t.Title;
            ws.Cell(row, 2).Value = t.Status;
            ws.Cell(row, 3).Value = t.Priority;
            ws.Cell(row, 4).Value = t.AssignedTo;

            if (t.DueDate.HasValue)
            {
                ws.Cell(row, 5).Value = t.DueDate.Value;                 // Date حقيقي
                ws.Cell(row, 5).Style.DateFormat.Format = "dd/MM/yyyy";  // شكل العرض
            }
            else
            {
                ws.Cell(row, 5).Value = "-";
            }

            var dataRange = ws.Range(row, 1, row, 5);
            dataRange.Style
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                .Alignment.SetWrapText(true);

            // Zebra
            if ((row - (headerRow + 1)) % 2 == 1)
                dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

            row++;
        }

        // ===== Layout =====
        ws.Column(1).Width = 35; // عنوان المهمة
        ws.Column(2).Width = 14; // الحالة
        ws.Column(3).Width = 14; // الأولوية
        ws.Column(4).Width = 22; // المسؤول
        ws.Column(5).Width = 16; // تاريخ الاستحقاق

        ws.SheetView.FreezeRows(headerRow);
        ws.Range(headerRow, 1, headerRow, 5).SetAutoFilter();

        // Print (اختياري)
        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.FitToPages(1, 0);

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
