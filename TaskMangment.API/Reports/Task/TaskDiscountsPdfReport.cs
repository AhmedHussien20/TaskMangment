using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using TaskMangment.Application.DTOs.ReportsDTO;

public class TaskDiscountsMovementPdfReport : IDocument
{
    private readonly List<TaskDiscountReportDto> _tasks;
    private readonly TaskMovementType _movementType;

    public TaskDiscountsMovementPdfReport(List<TaskDiscountReportDto> tasks, TaskMovementType movementType)
    {
        _tasks = tasks;
        _movementType = movementType;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page((Action<PageDescriptor>)(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.DefaultTextStyle(x => x.FontFamily("Cairo").FontSize(11));

            // ===== Header =====
            page.Header().PaddingBottom(10).Column((Action<ColumnDescriptor>)(column =>
            {
                string title = _movementType == TaskMangment.Application.DTOs.ReportsDTO.TaskMovementType.Incoming
                    ? "تقرير خصومات المهام الواردة"
                    : "تقرير خصومات المهام الصادرة";

                column.Item().AlignRight().Text(title).FontSize(20).Bold();
                column.Item().AlignRight().Text($"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}")
                      .FontSize(10).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            }));

            // ===== Content =====
            page.Content().PaddingTop(10).Table((Action<TableDescriptor>)(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // رقم المهمة + عنوانها
                    columns.RelativeColumn(3); // جهة التكليف
                    columns.RelativeColumn(3); // تاريخ الإغلاق
                    columns.RelativeColumn(2); // حالة المهمة
                    columns.RelativeColumn(2); // الخصومات التلقائية
                    columns.RelativeColumn(2); // الخصومات اليدوية
                    columns.RelativeColumn(2); // التقييم
                });

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

                string currentEmployee = null;
                int index = 0;

                foreach (var task in _tasks)
                {
                    // لو الصادر → لكل موظف صف أول فيه اسمه (centered)
                    if (_movementType == TaskMangment.Application.DTOs.ReportsDTO.TaskMovementType.Outgoing && currentEmployee != task.EmployeeName)
                    {
                        currentEmployee = task.EmployeeName;
                        table.Cell().ColumnSpan(7)
                             .Element(c => c
                                 .Border(1)
                                 .BorderColor(Colors.Grey.Lighten2)
                                 .Background(Colors.Grey.Lighten3)
                                 .Padding(6)
                                 .AlignCenter()
                             )
                             .Text(currentEmployee);
                        index = 0; // reset row index for alternating colors
                    }

                    string bgColor = index++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text($"[{task.TaskId}] {task.Title}");

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text(task.AssignedBy);

                    table.Cell().Element(c => DataCellStyle(c, bgColor))
                         .Text(task.ClosedDate.HasValue
                               ? task.ClosedDate.Value.ToString("yyyy/MM/dd")
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
            }));

            // ===== Footer =====
            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("صفحة ");
                t.CurrentPageNumber();
                t.Span(" من ");
                t.TotalPages();
            });
        }));
    }
    static IContainer HeaderCellStyle(IContainer container) =>
    container
        .Border(1)
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
