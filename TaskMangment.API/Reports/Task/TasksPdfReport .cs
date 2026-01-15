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

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

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
            page.ContentFromRightToLeft();

            // ================= HEADER =================
            page.Header().Column(column =>
            {
                column.Item().AlignRight()
                    .Text("Task Manager System")
                    .FontSize(9);

                column.Item().AlignCenter()
                    .Text("تقرير المهام")
                    .FontSize(20)
                    .Bold();

                column.Item().AlignCenter()
                    .Text($"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                column.Item().PaddingTop(5)
                    .LineHorizontal(1)
                    .LineColor(Colors.Grey.Lighten2);
            });

            // ================= CONTENT =================
            page.Content().PaddingTop(10).Column(column =>
            {
                // ===== TABLE =====
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);  
                        columns.RelativeColumn(2);  
                        columns.RelativeColumn(2);  
                        columns.RelativeColumn(3);  
                        columns.RelativeColumn(2);  
                    });

                    // ===== HEADER ROW =====
                    AddHeaderCell(table, "عنوان المهمة");
                    AddHeaderCell(table, "الحالة");
                    AddHeaderCell(table, "الأولوية");
                    AddHeaderCell(table, "المسؤول");
                    AddHeaderCell(table, "تاريخ الاستحقاق");

                    int index = 0;

                    foreach (var task in _tasks)
                    {
                        string bgColor = index++ % 2 == 0
                            ? Colors.White
                            : Colors.Grey.Lighten4;

                        AddDataCell(table, task.Title, bgColor);
                        AddDataCell(table, task.Status, bgColor);
                        AddDataCell(table, task.Priority, bgColor);
                        AddDataCell(table, task.AssignedTo, bgColor);
                        AddDataCell(
                            table,
                            task.DueDate.HasValue
                                ? task.DueDate.Value.ToString("dd/MM/yyyy")
                                : "-",
                            bgColor
                        );
                    }
                });
            });

            // ================= FOOTER =================
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Task Manager System — صفحة ");
                text.CurrentPageNumber();
                text.Span(" من ");
                text.TotalPages();
            });
        });
    }

    // ================= HELPERS =================

    void AddHeaderCell(TableDescriptor table, string text)
    {
        table.Cell()
            .Border(1)
            .BorderColor(Colors.Grey.Darken1)
            .Background(Colors.Grey.Lighten2)
            .Padding(6)
            .AlignRight()
            .Text(text)
            .Bold();
    }

    void AddDataCell(TableDescriptor table, string text, string bgColor)
    {
        table.Cell()
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(bgColor)
            .Padding(6)
            .AlignRight()
            .Text(text);
    }
}
