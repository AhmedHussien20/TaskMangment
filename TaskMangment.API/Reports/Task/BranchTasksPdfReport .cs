using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class BranchTasksPdfReport : IDocument
    {
        private readonly List<BranchTaskReportRowDto> _data;
        private readonly string _branchName;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;

        public BranchTasksPdfReport(
            List<BranchTaskReportRowDto> data,
            string branchName,
            DateTime fromDate,
            DateTime? toDate)
        {
            _data = data ?? new List<BranchTaskReportRowDto>();
            _branchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
            _fromDate = fromDate;
            _toDate = toDate ?? DateTime.Now;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
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
                        .Text("تقرير مهام الفرع")
                        .FontSize(18)
                        .Bold();

                    column.Item().AlignCenter()
                        .Text($"الفرع: {_branchName}")
                        .FontSize(10);

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
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);    // rank
                            columns.RelativeColumn(3.8f);  // task
                            columns.RelativeColumn(2f);    // assigned by
                            columns.RelativeColumn(3.8f);  // employees
                            columns.RelativeColumn(1.6f);  // status
                            columns.RelativeColumn(1.5f);  // created
                            columns.RelativeColumn(1.5f);  // due
                            columns.RelativeColumn(1.6f);  // extensions
                        });

                        // ===== HEADER =====
                        AddHeaderCell(table, "ترتيب");
                        AddHeaderCell(table, "المهمة");
                        AddHeaderCell(table, "جهة التكليف");
                        AddHeaderCell(table, "الموظفين");
                        AddHeaderCell(table, "الحالة");
                        AddHeaderCell(table, "تاريخ الإنشاء");
                        AddHeaderCell(table, "تاريخ الانتهاء");
                        AddHeaderCell(table, "عدد طلبات التمديد");

                        int rank = 1;

                        foreach (var item in _data
                                     .OrderBy(x => x.Title)
                                     .ThenBy(x => x.CreatedDate)
                                     .ThenBy(x => x.TaskId))
                        {
                            string bg = rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            var employeesText = (item.Employees == null || item.Employees.Count == 0)
                                ? "-"
                                : string.Join("، ", item.Employees.Select(e => e.Name));

                            AddDataCell(table, rank.ToString(), bg);
                            AddDataCell(table, $"[{item.TaskId}] {item.Title}", bg);
                            AddDataCell(table, item.AssignedBy ?? "غير معروف", bg);
                            AddDataCell(table, employeesText, bg);

                            AddDataCell(table, item.StatusText ?? "-", bg);
                            AddDataCell(table, item.CreatedDate.ToString("dd/MM/yyyy"), bg);

                            AddDataCell(table,
                                item.EffectiveDueDate.HasValue
                                    ? item.EffectiveDueDate.Value.ToString("dd/MM/yyyy")
                                    : "-",
                                bg);

                            AddDataCell(table, item.ExtensionRequestsCount.ToString(), bg);

                            rank++;
                        }
                    });
                });

                page.Footer().AlignCenter()
                    .Text($"Task Manager System — تم الإنشاء بتاريخ {DateTime.Now:dd/MM/yyyy}")
                    .FontSize(9);
            });
        }

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
                .Text(text ?? "-")
                .FontSize(9);
        }
    }
}
