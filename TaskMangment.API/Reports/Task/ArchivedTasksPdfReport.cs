using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class ArchivedTasksPdfReport : IDocument
    {
        private readonly List<EmployeeArchivedTasksReportDto> _data;

        public ArchivedTasksPdfReport(List<EmployeeArchivedTasksReportDto> data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            int totalEmployees = _data.Count;
            int totalTasks = _data.Sum(x => x.TotalTasks);
            int totalArchived = _data.Sum(x => x.ArchivedTasksCount);
            decimal maxArchiveRate = _data.Any() ? _data.Max(x => x.ArchiveRate) : 0;

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(x =>
                    x.FontFamily("Cairo")
                     .FontSize(11));

                // ================= HEADER =================
                page.Header().Column(column =>
                {
                    column.Item().AlignRight()
                        .Text("Task Manager System")
                        .FontSize(9);

                    column.Item().AlignCenter()
                        .Text("تقرير تحليل أرشفة المهام حسب الموظفين")
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
                    // ===== SUMMARY =====
                    column.Item().Row(row =>
                    {
                        SummaryCard(row, "عدد الموظفين", totalEmployees.ToString(), Colors.Blue.Lighten4);
                        SummaryCard(row, "إجمالي المهام", totalTasks.ToString(), Colors.Grey.Lighten3);
                        SummaryCard(row, "إجمالي المؤرشف", totalArchived.ToString(), Colors.Orange.Lighten4);
                        SummaryCard(row, "أعلى نسبة أرشفة", $"{maxArchiveRate:0.##}%", Colors.Red.Lighten4);
                    });

                    column.Item().PaddingTop(10);

                    // ===== TABLE =====
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);    
                            columns.RelativeColumn(4);    
                            columns.RelativeColumn(2);   
                            columns.RelativeColumn(2);   
                            columns.RelativeColumn(2);   
                        });

                        AddHeaderCell(table, "ترتيب");
                        AddHeaderCell(table, "اسم الموظف");
                        AddHeaderCell(table, "إجمالي المهام");
                        AddHeaderCell(table, "المؤرشفة");
                        AddHeaderCell(table, "نسبة الأرشفة %");

                        int index = 1;

                        foreach (var row in _data.OrderByDescending(x => x.ArchiveRate))
                        {
                            string bg = Colors.White;

                            AddDataCell(table, index.ToString(), bg);
                            AddDataCell(table, row.EmployeeName, bg);
                            AddDataCell(table, row.TotalTasks.ToString(), bg);
                            AddDataCell(table, row.ArchivedTasksCount.ToString(), bg);

                            string rateColor =
                                row.ArchiveRate > 50 ? Colors.Red.Lighten4 :
                                row.ArchiveRate >= 20 ? Colors.Orange.Lighten4 :
                                Colors.Green.Lighten4;

                            table.Cell()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Background(rateColor)
                                .Padding(6)
                                .AlignCenter()
                                .Text($"{row.ArchiveRate:0.##}%");

                            index++;
                        }
                    });

                    // ===== LEGEND =====
                    column.Item().PaddingTop(8)
                        .AlignRight()
                        .Text("🟥 نسبة عالية   🟧 متوسطة   🟩 طبيعية")
                        .FontSize(9);
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

        void SummaryCard(RowDescriptor row, string title, string value, string bgColor)
        {
            row.RelativeItem()
               .Background(bgColor)
               .Border(1)
               .Padding(8)
               .AlignCenter()
               .Column(c =>
               {
                   c.Item().Text(title).Bold().FontSize(10);
                   c.Item().Text(value).Bold().FontSize(16);
               });
        }

        void AddHeaderCell(TableDescriptor table, string text)
        {
            table.Cell()
                .Border(1)
                .BorderColor(Colors.Grey.Darken1)
                .Background(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignCenter()
                .Text(text)
                .Bold();
        }

        void AddDataCell(TableDescriptor table, string text, string bg)
        {
            table.Cell()
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(bg)
                .Padding(6)
                .AlignCenter()
                .Text(text);
        }
    }
}
