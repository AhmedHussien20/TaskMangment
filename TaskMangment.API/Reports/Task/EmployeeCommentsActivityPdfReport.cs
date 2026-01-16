using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class EmployeeCommentsActivityPdfReport : IDocument
    {
        private readonly List<EmployeeCommentsActivityReportDto> _data;
        private readonly DateTime _fromDate;
        private readonly DateTime? _toDate;

        public EmployeeCommentsActivityPdfReport(
            List<EmployeeCommentsActivityReportDto> data,
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
            int totalComments = _data.Sum(x => x.TotalComments);
            var topEmployee = _data.OrderByDescending(x => x.TotalComments).FirstOrDefault();
            var lastActiveDate = _data.Max(x => x.LastCommentDate);

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
                        .Text("تقرير نشاط الموظفين والتفاعل مع المهام")
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
                            c.Item().Text("إجمالي التعليقات").Bold().FontSize(9);
                            c.Item().Text(totalComments.ToString()).Bold().FontSize(16);
                        });

                        row.RelativeItem().Background(Colors.Orange.Lighten4).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("أكثر تفاعل").Bold().FontSize(9);
                            c.Item().Text(topEmployee != null
                                ? $"{topEmployee.EmployeeName} ({topEmployee.TotalComments})"
                                : "-")
                                .Bold().FontSize(12);
                        });

                        row.RelativeItem().Background(Colors.Grey.Lighten3).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("آخر نشاط").Bold().FontSize(9);
                            c.Item().Text(lastActiveDate.HasValue
                                ? lastActiveDate.Value.ToString("dd/MM/yyyy")
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
                        AddHeaderCell(table, "إجمالي التعليقات");
                        AddHeaderCell(table, "عدد المهام");
                        AddHeaderCell(table, "متوسط / مهمة");
                        AddHeaderCell(table, "آخر تعليق");

                        int rank = 1;

                        foreach (var row in _data.OrderByDescending(x => x.TotalComments))
                        {
                            string bg = rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            AddDataCell(table, rank.ToString(), bg);
                            AddDataCell(table, row.EmployeeName, bg);
                            AddDataCell(table, row.TotalComments.ToString(), bg);
                            AddDataCell(table, row.DistinctTasksCount.ToString(), bg);
                            AddDataCell(table, row.AvgCommentsPerTask.ToString("0.##"), bg);
                            AddDataCell(table, row.LastCommentDate?.ToString("dd/MM/yyyy") ?? "-", bg);

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
