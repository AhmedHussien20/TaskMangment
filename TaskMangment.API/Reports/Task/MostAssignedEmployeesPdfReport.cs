using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class MostAssignedEmployeesPdfReport : IDocument
    {
        private readonly List<EmployeeAssignmentsReportDto> _data;
        private readonly DateTime _fromDate;
        private readonly DateTime? _toDate;

        public MostAssignedEmployeesPdfReport(
            List<EmployeeAssignmentsReportDto> data,
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
            int totalTasks = _data.Sum(x => x.TotalTasks);
            var topEmployee = _data.OrderByDescending(x => x.TotalTasks).FirstOrDefault();
            var leastEmployee = _data.OrderBy(x => x.TotalTasks).FirstOrDefault();

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
                        .Text("تقرير توزيع المهام على الموظفين")
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
                            c.Item().Text("إجمالي المهام").Bold().FontSize(9);
                            c.Item().Text(totalTasks.ToString()).Bold().FontSize(16);
                        });

                        row.RelativeItem().Background(Colors.Orange.Lighten4).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("أعلى عبء عمل").Bold().FontSize(9);
                            c.Item().Text(topEmployee != null
                                ? $"{topEmployee.EmployeeName} ({topEmployee.TotalTasks})"
                                : "-")
                                .Bold().FontSize(12);
                        });

                        row.RelativeItem().Background(Colors.Grey.Lighten3).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("أقل عبء عمل").Bold().FontSize(9);
                            c.Item().Text(leastEmployee != null
                                ? $"{leastEmployee.EmployeeName} ({leastEmployee.TotalTasks})"
                                : "-")
                                .Bold().FontSize(12);
                        });
                    });

                    // ================= TABLE =================
                    column.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);  
                            columns.RelativeColumn(3);  
                            columns.RelativeColumn(1.5f); 
                            columns.RelativeColumn(1.5f); 
                            columns.RelativeColumn(1.5f); 
                            columns.RelativeColumn(1.5f);  
                            columns.RelativeColumn(1.5f); 
                            columns.RelativeColumn(1.5f);  
                            columns.RelativeColumn(2); 
                        });

                        // ===== HEADER =====
                        AddHeaderCell(table, "ترتيب");
                        AddHeaderCell(table, "الموظف");
                        AddHeaderCell(table, "الإجمالي");
                        AddHeaderCell(table, "جديدة");
                        AddHeaderCell(table, "قيد التنفيذ");
                        AddHeaderCell(table, "مغلقة");
                        AddHeaderCell(table, "متأخرة");
                        AddHeaderCell(table, "قريبة");
                        AddHeaderCell(table, "نسبة الإنجاز");

                        int rank = 1;

                        foreach (var row in _data.OrderByDescending(x => x.TotalTasks))
                        {
                            string bg = rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            AddDataCell(table, rank.ToString(), bg);
                            AddDataCell(table, row.EmployeeName, bg);
                            AddDataCell(table, row.TotalTasks.ToString(), bg);
                            AddDataCell(table, row.NewTasks.ToString(), bg);
                            AddDataCell(table, row.InProgressTasks.ToString(), bg);
                            AddDataCell(table, row.ClosedTasks.ToString(), bg);
                            AddDataCell(table, row.OverdueTasks.ToString(), bg);
                            AddDataCell(table, row.ClosingSoonTasks.ToString(), bg);
                            AddDataCell(table, row.CompletionRate.ToString("0.##") + " %", bg);

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