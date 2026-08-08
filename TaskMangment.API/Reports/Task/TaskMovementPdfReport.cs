using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class TaskMovementPdfReport : IDocument
    {
        private readonly List<TaskMovementReportDto> _items;
        private readonly TaskMovementType _movementType;

        public TaskMovementPdfReport(
            List<TaskMovementReportDto> items,
             TaskMovementType movementType)
        {
            _items = items;
            _movementType = movementType;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontFamily("Cairo").FontSize(11));

                // ================= Header =================
                page.Header().PaddingBottom(10).Column(column =>
                {
                    string title = _movementType == TaskMangment.Application.DTOs.ReportsDTO.TaskMovementType.Incoming
                    ? "تقرير حركة المهام الواردة لليوم"
                    : "تقرير حركة المهام الصادرة لليوم";

                    column.Item()
                        .AlignRight()
                        .Text(title)
                        .FontSize(20)
                        .Bold();
                   

                    var subTitle = _items.FirstOrDefault()?.ReportTitle;
                    if (!string.IsNullOrWhiteSpace(subTitle))
                    {
                        column.Item()
                            .AlignRight()
                            .Text(subTitle)
                            .FontSize(13)
                            .FontColor(Colors.Grey.Darken2);
                    }

                    column.Item()
                        .AlignRight()
                        .Text($"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item()
                        .PaddingTop(5)
                        .LineHorizontal(1)
                        .LineColor(Colors.Grey.Lighten2);
                });

                // ================= Content =================
                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); 
                        columns.RelativeColumn(3);  
                        columns.RelativeColumn(4);  
                        columns.RelativeColumn(2);  
                        columns.RelativeColumn(3); 
                    });

                    // ===== Table Header =====
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("المهمة");
                        header.Cell().Element(HeaderCellStyle).Text("جهة التكليف");
                        header.Cell().Element(HeaderCellStyle).Text("التعليق");
                        header.Cell().Element(HeaderCellStyle).Text("تاريخ التعليق");
                        header.Cell().Element(HeaderCellStyle).Text("الموظف الذي علّق");
                    });

                    // ===== Table Data =====
                    int index = 0;
                    foreach (var item in _items)
                    {
                        string bgColor = index++ % 2 == 0
                            ? Colors.White
                            : Colors.Grey.Lighten4;

                        table.Cell().Element(c => DataCellStyle(c, bgColor))
                            .Text(item.TaskTitleWithId);

                        table.Cell().Element(c => DataCellStyle(c, bgColor))
                            .Text(item.AssignedBy);

                        table.Cell().Element(c => DataCellStyle(c, bgColor))
                            .Text(item.CommentText);

                        table.Cell().Element(c => DataCellStyle(c, bgColor))
                            .Text(item.CommentDate.ToString("yyyy/MM/dd HH:mm"));

                        table.Cell().Element(c => DataCellStyle(c, bgColor))
                            .Text(item.CommentedBy);
                    }
                });

                // ================= Footer =================
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("صفحة ");
                    text.CurrentPageNumber();
                    text.Span(" من ");
                    text.TotalPages();
                });
            });
        }

        // ================= Styles =================
        static IContainer HeaderCellStyle(IContainer container) =>
            container
                .Border(1)
                .BorderColor(Colors.Grey.Darken1)
                .Background(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignRight()
                .DefaultTextStyle(x => x.Bold());

        static IContainer DataCellStyle(IContainer container, string backgroundColor) =>
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(backgroundColor)
                .Padding(6)
                .AlignRight();
    }
}
