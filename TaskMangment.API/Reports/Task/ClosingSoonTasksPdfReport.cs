using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class ClosingSoonTasksPdfReport : IDocument
    {
        private readonly List<TasksClosingSoonDto> _tasks;

        public ClosingSoonTasksPdfReport(List<TasksClosingSoonDto> tasks)
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

                // ===== HEADER =====
                page.Header().PaddingBottom(10).Column(column =>
                {
                    column.Item().AlignRight()
                        .Text("المهام الجديدة وقيد التنفيذ التي ستغلق قريبًا")
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

                // ===== CONTENT =====
                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // رقم المهمة
                        columns.RelativeColumn(5); // عنوان المهمة
                        columns.RelativeColumn(3); // جهة التكليف
                        columns.RelativeColumn(2); // تاريخ الإغلاق المتوقع
                    });

                    // ===== Table Header =====
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("رقم المهمة");
                        header.Cell().Element(HeaderCellStyle).Text("عنوان المهمة");
                        header.Cell().Element(HeaderCellStyle).Text("جهة التكليف");
                        header.Cell().Element(HeaderCellStyle).Text("تاريخ الإغلاق المتوقع");
                    });

                    // ===== Table Data =====
                    int index = 0;
                    foreach (var task in _tasks)
                    {
                        string bg = index++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Element(c => DataCellStyle(c, bg))
                            .Text(task.TaskId.ToString());

                        table.Cell().Element(c => DataCellStyle(c, bg))
                            .Text(task.Title);

                        table.Cell().Element(c => DataCellStyle(c, bg))
                            .Text(task.AssignedBy);

                        table.Cell().Element(c => DataCellStyle(c, bg))
                            .Text(task.ClosedDate.HasValue
                                ? task.ClosedDate.Value.ToString("yyyy/MM/dd")
                                : "-");
                    }
                });

                // ===== FOOTER =====
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("صفحة ");
                    text.CurrentPageNumber();
                    text.Span(" من ");
                    text.TotalPages();
                });
            });
        }

        static IContainer HeaderCellStyle(IContainer container) =>
            container
                .Border(1)
                .BorderColor(Colors.Grey.Darken1)
                .Background(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignRight()
                .DefaultTextStyle(x => x.Bold());

        static IContainer DataCellStyle(IContainer container, string bg) =>
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(bg)
                .Padding(6)
                .AlignRight();
    }
}
