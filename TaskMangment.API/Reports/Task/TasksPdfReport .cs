using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.ReportDTOs;

public class TasksPdfReport : IDocument
{
    private readonly List<TaskReportDto> _tasks;

    public TasksPdfReport(List<TaskReportDto> tasks)
    {
        _tasks = tasks;
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            // ================= PAGE =================
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.DefaultTextStyle(x =>
                x.FontFamily("Cairo")
                 .FontSize(11));

            // ================= HEADER =================
            page.Header().PaddingBottom(10).Column(column =>
            {
                column.Item().AlignRight()
                    .Text("تقرير المهام")
                    .FontSize(20)
                    .Bold();

                column.Item().AlignRight()
                    .Text($"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                column.Item().PaddingTop(5)
                    .LineHorizontal(1)
                    .LineColor(Colors.Grey.Lighten2);
            });

            // ================= CONTENT =================
            page.Content().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); 
                    columns.RelativeColumn(2);  
                    columns.RelativeColumn(2);  
                    columns.RelativeColumn(3);  
                    columns.RelativeColumn(2); 
                });

                // ---------- TABLE HEADER ----------
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("عنوان المهمة");
                    header.Cell().Element(HeaderCellStyle).Text("الحالة");
                    header.Cell().Element(HeaderCellStyle).Text("الأولوية");
                    header.Cell().Element(HeaderCellStyle).Text("المسؤول");
                    header.Cell().Element(HeaderCellStyle).Text("تاريخ الاستحقاق");
                });

                // ---------- TABLE ROWS ----------
                int index = 0;
                foreach (var task in _tasks)
                {
                    string bgColor = index++ % 2 == 0
                        ? Colors.White
                        : Colors.Grey.Lighten4;

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                        .Text(task.Title);

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                        .Text(task.Status);

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                        .Text(task.Priority);

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                        .Text(task.AssignedTo);

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                        .Text(task.DueDate?.ToString("yyyy/MM/dd") ?? "-");
                }
            });

            // ================= FOOTER =================
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("صفحة ");
                text.CurrentPageNumber();
                text.Span(" من ");
                text.TotalPages();
            });
        });
    }

    // ================= STYLES =================

    static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Darken1)
            .Background(Colors.Grey.Lighten2)
            .Padding(6)
            .AlignRight()
            .DefaultTextStyle(x => x.Bold());
    }

    static IContainer DataCellStyle(IContainer container, string backgroundColor)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(backgroundColor)
            .Padding(6)
            .AlignRight();
    }
}
