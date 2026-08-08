using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.Infrastructure.PDF
{
    public class MonthlyEmployeeTotalDiscountPdfDocument : IDocument
    {
        private readonly List<EmployeeTotalDiscountReportRowDto> _data;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;
        private readonly string? _branchName;

        public MonthlyEmployeeTotalDiscountPdfDocument(
            List<EmployeeTotalDiscountReportRowDto> data,
            DateTime fromDate,
            DateTime toDate,
            string? branchName = null)
        {
            _data = data;
            _fromDate = fromDate;
            _toDate = toDate;
            _branchName = branchName;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(10));
                page.ContentFromRightToLeft();

                page.Header().Column(column =>
                {
                    column.Item().AlignRight().Text("Task Manager System").FontSize(9);
                    column.Item().AlignCenter().Text(BuildTitle()).FontSize(18).Bold();
                    if (!string.IsNullOrWhiteSpace(_branchName))
                    {
                        column.Item().AlignCenter()
                            .Text($"الفرع: {_branchName}")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken2);
                    }
                    column.Item().AlignCenter()
                        .Text($"الفترة: من {_fromDate:yyyy/MM/dd} إلى {_toDate:yyyy/MM/dd}")
                        .FontSize(10);
                    column.Item().AlignCenter()
                        .Text($"تاريخ إنشاء التقرير: {DateTime.Now:yyyy/MM/dd}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).LineHorizontal(1);
                });

                page.Content().PaddingTop(10).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Background(Colors.Red.Lighten4)
                            .Border(1)
                            .Padding(8)
                            .AlignCenter()
                            .Column(c =>
                            {
                                c.Item().Text("إجمالي الخصومات").Bold().FontSize(9);
                                c.Item().Text(_data.Sum(x => x.TotalDiscount).ToString("0.##")).Bold().FontSize(16);
                            });
                    });

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        AddHeader(table, "م");
                        AddHeader(table, "الموظف");
                        AddHeader(table, "الصلاحية");
                        AddHeader(table, "إجمالي الخصومات");

                        var index = 1;
                        foreach (var item in _data)
                        {
                            var bg = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                            AddCell(table, index.ToString(), bg);
                            AddCell(table, item.EmployeeName ?? "غير معروف", bg);
                            AddCell(table, item.RoleTitle ?? "-", bg);
                            AddCell(table, item.TotalDiscount.ToString("0.##"), bg);
                            index++;
                        }
                    });
                });

                page.Footer().AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Task Manager System - صفحة ");
                        t.CurrentPageNumber();
                        t.Span(" من ");
                        t.TotalPages();
                    });
            });
        }

        private string BuildTitle() => "خصومات الموظفين طوال الشهر";

        private static void AddHeader(TableDescriptor table, string text)
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

        private static void AddCell(TableDescriptor table, string text, string bg)
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
