using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class EmployeeTaskTrackingPdfReport : IDocument
    {
        private readonly List<EmployeeTaskTrackingReportDto> _data;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;

        public EmployeeTaskTrackingPdfReport(
            List<EmployeeTaskTrackingReportDto> data,
            DateTime fromDate,
            DateTime? toDate)
        {
            _data = data;
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
                        .Text("تقرير تتبع مهام الموظف")
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
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);     // rank
                            columns.RelativeColumn(2);      // employee
                            columns.RelativeColumn(4);      // المهمة (أوسع)
                            columns.RelativeColumn(1.5f);   // status
                            columns.RelativeColumn(1.5f);   // created
                            columns.RelativeColumn(1.5f);   // closed
                            columns.RelativeColumn(2);      // assigned by
                        });

                        // ===== HEADER =====
                        AddHeaderCell(table, "ترتيب");
                        AddHeaderCell(table, "الموظف");
                        AddHeaderCell(table, "المهمة");        // ✅
                        AddHeaderCell(table, "الحالة");
                        AddHeaderCell(table, "تاريخ الإنشاء");
                        AddHeaderCell(table, "تاريخ الإغلاق");
                        AddHeaderCell(table, "جهة التكليف");

                        int rank = 1;

                        foreach (var item in _data
                                     .OrderBy(x => x.EmployeeName)
                                     .ThenBy(x => x.CreatedDate))
                        {
                            string bg = rank % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            AddDataCell(table, rank.ToString(), bg);
                            AddDataCell(table, item.EmployeeName ?? "غير معروف", bg);

                            // ✅ [id] title
                            AddDataCell(table, $"[{item.TaskId}] {item.Title}", bg);

                            AddDataCell(table, item.Status ?? "-", bg);
                            AddDataCell(table, item.CreatedDate.ToString("dd/MM/yyyy"), bg);
                            AddDataCell(table, item.ClosedAt.HasValue ? item.ClosedAt.Value.ToString("dd/MM/yyyy") : "-", bg);
                            AddDataCell(table, item.AssignedBy ?? "غير معروف", bg);

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
                .Text(text)
                .FontSize(9);
        }
    }
}
