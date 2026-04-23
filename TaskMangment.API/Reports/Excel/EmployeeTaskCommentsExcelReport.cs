using ClosedXML.Excel;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Excel
{
    public static class EmployeeTaskCommentsExcelReport
    {
        public static byte[] Build(List<EmployeeTaskCommentRowDto> data)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Employee Task Comments");

            ws.RightToLeft = true;

            ws.Cell(1, 1).Value = "تقرير تعليقات الموظف على المهمة";
            ws.Range(1, 1, 1, 3).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(2, 1).Value = $"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}";
            ws.Range(2, 1, 2, 3).Merge().Style
                .Font.SetFontSize(10)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            int headerRow = 4;
            ws.Cell(headerRow, 1).Value = "ترتيب";
            ws.Cell(headerRow, 2).Value = "تاريخ التعليق";
            ws.Cell(headerRow, 3).Value = "التعليق";

            var headerRange = ws.Range(headerRow, 1, headerRow, 3);
            headerRange.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E5E5E5"))
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

            int row = headerRow + 1;
            int rank = 1;
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];

                ws.Cell(row, 1).Value = rank++;
                ws.Cell(row, 2).Value = item.CommentDate.ToString("yyyy/MM/dd");
                ws.Cell(row, 3).Value = item.CommentText;

                var dataRange = ws.Range(row, 1, row, 3);
                dataRange.Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Top)
                    .Alignment.SetWrapText(true);

                if (i % 2 == 1)
                    dataRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F5F5F5"));

                row++;
            }

            ws.Column(1).Width = 8;
            ws.Column(2).Width = 18;
            ws.Column(3).Width = 60;

            ws.SheetView.FreezeRows(headerRow);
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 0);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

