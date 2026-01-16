using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class OnTimeCompletionPdfReport : IDocument
    {
        private readonly List<EmployeeOnTimeReportDto> _data;
        private readonly DateTime _fromDate;
        private readonly DateTime? _toDate;

        public OnTimeCompletionPdfReport(
            List<EmployeeOnTimeReportDto> data,
            DateTime fromDate,
            DateTime? toDate)
        {
            _data = data;
            _fromDate = fromDate;
            _toDate = toDate;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            int employeesCount = _data.Count;
            int totalClosedTasks = _data.Sum(x => x.TotalClosedTasks);
            var bestEmployee = _data.OrderByDescending(x => x.CommitmentPercentage).FirstOrDefault();
            var worstEmployee = _data.OrderBy(x => x.CommitmentPercentage).FirstOrDefault();

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(x => x.FontFamily("Cairo").FontSize(10));

                // ================= HEADER =================
                page.Header().Column(column =>
                {
                    column.Item().AlignRight().Text("Task Manager System").FontSize(9);

                    column.Item().AlignCenter()
                        .Text("تقرير أفضل الموظفين التزامًا بالمواعيد")
                        .FontSize(18)
                        .Bold();

                    column.Item().AlignCenter()
                        .Text($"الفترة: من {_fromDate:dd/MM/yyyy} إلى {_toDate:dd/MM/yyyy}")
                        .FontSize(10);

                    column.Item().AlignCenter()
                        .Text($"تاريخ إنشاء التقرير: {DateTime.Now:dd/MM/yyyy}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(5).LineHorizontal(1);
                });

                // ================= CONTENT =================
                page.Content().PaddingTop(10).Column(column =>
                {
                    // ================= SUMMARY =================
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Blue.Lighten4).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("عدد الموظفين").Bold().FontSize(9);
                            c.Item().Text(employeesCount.ToString()).Bold().FontSize(16);
                        });

                        row.RelativeItem().Background(Colors.Green.Lighten4).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("إجمالي المهام المغلقة").Bold().FontSize(9);
                            c.Item().Text(totalClosedTasks.ToString()).Bold().FontSize(16);
                        });

                        row.RelativeItem().Background(Colors.Orange.Lighten4).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("أفضل التزام").Bold().FontSize(9);
                            c.Item().Text(bestEmployee != null
                                ? $"{bestEmployee.EmployeeName} ({bestEmployee.CommitmentPercentage:0.##}%)"
                                : "-")
                                .Bold().FontSize(12);
                        });

                        row.RelativeItem().Background(Colors.Red.Lighten4).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("أقل التزام").Bold().FontSize(9);
                            c.Item().Text(worstEmployee != null
                                ? $"{worstEmployee.EmployeeName} ({worstEmployee.CommitmentPercentage:0.##}%)"
                                : "-")
                                .Bold().FontSize(12);
                        });
                    });

                    // ================= TABLE =================
                    column.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(2);  
                            columns.RelativeColumn(2);   
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2);  
                        });

                        AddHeaderCell(table, "ترتيب");
                        AddHeaderCell(table, "الموظف");
                        AddHeaderCell(table, "في الوقت");
                        AddHeaderCell(table, "متأخرة");
                        AddHeaderCell(table, "إجمالي المغلقة");
                        AddHeaderCell(table, "نسبة الالتزام %");

                        int rank = 1;

                        foreach (var row in _data.OrderByDescending(x => x.CommitmentPercentage))
                        {
                            string bg = rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            string percentBg = row.CommitmentPercentage >= 90
                                ? Colors.Green.Lighten4
                                : row.CommitmentPercentage >= 70
                                    ? Colors.Orange.Lighten4
                                    : Colors.Red.Lighten4;

                            AddDataCell(table, rank.ToString(), bg);
                            AddDataCell(table, row.EmployeeName, bg);
                            AddDataCell(table, row.OnTimeTasks.ToString(), bg);
                            AddDataCell(table, row.LateTasks.ToString(), bg);
                            AddDataCell(table, row.TotalClosedTasks.ToString(), bg);

                            table.Cell()
                                .Border(1)
                                .Background(percentBg)
                                .Padding(3)
                                .AlignRight()
                                .Text(row.CommitmentPercentage.ToString("0.##") + " %")
                                .FontSize(9);

                            rank++;
                        }
                    });
                });

                // ================= FOOTER =================
                page.Footer().AlignCenter()
                    .Text($"Task Manager System — تم الإنشاء بتاريخ {DateTime.Now:dd/MM/yyyy}")
                    .FontSize(9);
            });
        }

        // ================= HELPERS =================

        void AddHeaderCell(TableDescriptor table, string text)
        {
            table.Cell()
                .Border(1)
                .Background(Colors.Blue.Lighten5)
                .Padding(4)
                .AlignRight()
                .Text(text)
                .Bold()
                .FontSize(9);
        }

        void AddDataCell(TableDescriptor table, string text, string bg)
        {
            table.Cell()
                .Border(1)
                .Background(bg)
                .Padding(3)
                .AlignRight()
                .Text(text)
                .FontSize(9);
        }
    }
}
