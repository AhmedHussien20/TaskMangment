using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using TaskMangment.Application.DTOs.ReportsDTO;

public class TaskDiscountsPdfReport : IDocument
{
    private readonly List<TaskDiscountReportDto> _tasks;

    public TaskDiscountsPdfReport(List<TaskDiscountReportDto> tasks)
    {
        _tasks = tasks;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontFamily("Cairo").FontSize(11));

            // ================= HEADER =================
            page.Header().PaddingBottom(10).Column(column =>
            {
                column.Item().AlignRight().Text("تقرير خصومات المهام الواردة").FontSize(20).Bold();
                column.Item().AlignRight().Text($"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}")
                      .FontSize(10).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });

            // ================= CONTENT =================
            page.Content().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // رقم المهمة + عنوان المهمة
                    columns.RelativeColumn(3); // جهة التكليف
                    columns.RelativeColumn(3); // تاريخ الإغلاق
                    columns.RelativeColumn(2); // حالة المهمة
                    columns.RelativeColumn(2); // الخصومات التلقائية
                    columns.RelativeColumn(2); // الخصومات اليدوية
                    columns.RelativeColumn(2); // التقييم
                });

                // ---------- TABLE HEADER ----------
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("رقم المهمة / العنوان");
                    header.Cell().Element(HeaderCellStyle).Text("جهة التكليف");
                    header.Cell().Element(HeaderCellStyle).Text("تاريخ الإغلاق");
                    header.Cell().Element(HeaderCellStyle).Text("حالة المهمة");
                    header.Cell().Element(HeaderCellStyle).Text("الخصومات التلقائية");
                    header.Cell().Element(HeaderCellStyle).Text("الخصومات اليدوية");
                    header.Cell().Element(HeaderCellStyle).Text("التقييم");
                });

                // ---------- EMPLOYEE ROW (أول صف) ----------
                if (_tasks.Any())
                {
                    string bgColor = Colors.Grey.Lighten3;

                    table.Cell().ColumnSpan(7)
                         .Element(c => c
                             .Border(1)
                             .BorderColor(Colors.Grey.Lighten2)
                             .Background(bgColor)
                             .Padding(6)
                             .AlignCenter() 
                         )
                         .Text($"{_tasks.First().EmployeeName}");
                }

                // ---------- TABLE ROWS ----------
                int index = 0;
                foreach (var task in _tasks)
                {
                    string bgColor = index++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text($"[{task.TaskId}] {task.Title}");

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text(task.AssignedBy);

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
     .Text(task.ClosedDate.HasValue
           ? task.ClosedDate.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)
           : "-");

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text(task.Status);

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text(task.AutoDiscount.ToString("0.##"));

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text(task.ManualDiscount.ToString("0.##"));

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text(task.Evaluation);
                }
            });

            // ================= FOOTER =================
            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("صفحة ");
                t.CurrentPageNumber();
                t.Span(" من ");
                t.TotalPages();
            });
        });
    }

    // ================= STYLES =================
    static IContainer HeaderCellStyle(IContainer container) =>
        container.Border(1)
                 .BorderColor(Colors.Grey.Darken1)
                 .Background(Colors.Grey.Lighten2)
                 .Padding(6)
                 .AlignRight()
                 .DefaultTextStyle(x => x.Bold());

    static IContainer DataCellStyle(IContainer container, string backgroundColor) =>
        container.Border(1)
                 .BorderColor(Colors.Grey.Lighten2)
                 .Background(backgroundColor)
                 .Padding(6)
                 .AlignRight();
}
