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
            int totalTasks = _tasks.Count;
            int affectedEmployees = _tasks.Select(t => t.EmployeeName).Distinct().Count();
            int affectedBranches = _tasks.Select(t => t.BranchName).Distinct().Count();

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.ContentFromRightToLeft();

                page.DefaultTextStyle(x =>
                    x.FontFamily("Cairo").FontSize(10));

                // ================= HEADER =================
                page.Header().Column(column =>
                {
                    column.Item().AlignRight().Text("Task Manager System").FontSize(9);

                    column.Item().AlignCenter()
                        .Text("تقرير المهام القريبة من الإغلاق")
                        .FontSize(18)
                        .Bold();

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
                            c.Item().Text("عدد المهام القريبة من الإغلاق").Bold().FontSize(9);
                            c.Item().Text(totalTasks.ToString()).Bold().FontSize(16);
                        });

                        row.RelativeItem().Background(Colors.Green.Lighten4).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("الموظفون المتأثرون").Bold().FontSize(9);
                            c.Item().Text(affectedEmployees.ToString()).Bold().FontSize(16);
                        });

                        row.RelativeItem().Background(Colors.Grey.Lighten3).Border(1).Padding(8).AlignCenter().Column(c =>
                        {
                            c.Item().Text("الفروع المتأثرة").Bold().FontSize(9);
                            c.Item().Text(affectedBranches.ToString()).Bold().FontSize(16);
                        });
                    });

                    // ================= TABLE =================
                    column.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);    
                            columns.RelativeColumn(3);    
                            columns.RelativeColumn(2);   
                            columns.RelativeColumn(2);  
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2);  
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(1.5f); 
                        });

                        // ===== HEADER =====
                        AddHeaderCell(table, "رقم");
                        AddHeaderCell(table, "عنوان المهمة");
                        AddHeaderCell(table, "الموظف");
                        AddHeaderCell(table, "الشركة");
                        AddHeaderCell(table, "الفرع");
                        AddHeaderCell(table, "المنطقة");
                        AddHeaderCell(table, "جهة التكليف");
                        AddHeaderCell(table, "تاريخ الإغلاق المتوقع");
                        AddHeaderCell(table, "الحالة");

                        int index = 1;

                        foreach (var task in _tasks.OrderBy(t => t.ClosedDate))
                        {
                            string bg = index % 2 == 0
                                ? Colors.Grey.Lighten4
                                : Colors.White;

                            AddDataCell(table, index++.ToString(), bg);
                            AddDataCell(table, task.Title, bg);
                            AddDataCell(table, task.EmployeeName, bg);
                            AddDataCell(table, task.CompanyName, bg);
                            AddDataCell(table, task.BranchName, bg);
                            AddDataCell(table, task.AreaName, bg);
                            AddDataCell(table, task.AssignedBy, bg);
                            AddDataCell(
                                table,
                                task.DueDate.HasValue
                                    ? task.DueDate.Value.ToString("dd/MM/yyyy")
                                    : "-",
                                bg);
                            AddDataCell(table, task.Status.ToString(), bg);
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
