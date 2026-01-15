using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using TaskMangment.Application.DTOs.ReportsDTO;

public class TaskDiscountAuditPdfReport : IDocument
{
    private readonly TaskDiscountAuditReportDto _report;
    private readonly string _title;

    public TaskDiscountAuditPdfReport(TaskDiscountAuditReportDto report, string title)
    {
        _report = report;
        _title = title;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontFamily("Cairo").FontSize(10));
            page.ContentFromRightToLeft();

            // ================= HEADER =================
            page.Header().Column(column =>
            {
                column.Item().AlignRight().Text("Task Manager System").FontSize(9);

                column.Item().AlignCenter()
                    .Text(_title)
                    .FontSize(18)
                    .Bold();

                column.Item().AlignCenter()
                    .Text($"الفترة: من {_report.FromDate:yyyy/MM/dd} إلى {_report.ToDate:yyyy/MM/dd}")
                    .FontSize(10);

                column.Item().AlignCenter()
                    .Text($"تاريخ إنشاء التقرير: {DateTime.Now:yyyy/MM/dd}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);

                column.Item().PaddingTop(5).LineHorizontal(1);
            });

            // ================= CONTENT =================
            page.Content().PaddingTop(10).Column(column =>
            {
                // ===== SUMMARY =====
                column.Item().Row(row =>
                {
                    SummaryBox(row, "إجمالي الخصومات", _report.GrandTotal.ToString("0.##"), Colors.Red.Lighten4);
                    SummaryBox(row, "الخصومات التلقائية", _report.GrandTotalAuto.ToString("0.##"), Colors.Blue.Lighten4);
                    SummaryBox(row, "الخصومات اليدوية", _report.GrandTotalManual.ToString("0.##"), Colors.Orange.Lighten4);
                    SummaryBox(row, "عدد المهام", _report.TotalTasks.ToString(), Colors.Green.Lighten4);
                });

                column.Item().PaddingTop(10);

                // ===== GROUPS =====
                foreach (var group in _report.Groups)
                {
                    column.Item()
                             .Background(Colors.Blue.Lighten4)
                             .Padding(6)
                             .AlignRight()
                             .Text($"الموظف: {group.EmployeeName}")
                             .Bold();


                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        AddHeader(table, "م");
                        AddHeader(table, "المهمة");
                        AddHeader(table, "جهة التكليف");
                        AddHeader(table, "تاريخ الإغلاق");
                        AddHeader(table, "الحالة");
                        AddHeader(table, "تلقائي");
                        AddHeader(table, "يدوي");

                        int i = 1;
                        foreach (var task in group.Tasks)
                        {
                            AddCell(table, i++.ToString());
                            AddCell(table, $"[{task.TaskId}] {task.Title}");
                            AddCell(table, task.AssignedBy);
                            AddCell(table, task.ClosedDate?.ToString("yyyy/MM/dd") ?? "-");
                            AddCell(table, task.Status);
                            AddCell(table, task.AutoDiscount.ToString("0.##"), task.AutoDiscount > 0 ? Colors.Blue.Lighten5 : Colors.White);
                            AddCell(table, task.ManualDiscount.ToString("0.##"), task.ManualDiscount > 0 ? Colors.Orange.Lighten5 : Colors.White);
                        }
                    });

                    column.Item()
                            .Background(Colors.Yellow.Lighten4)
                            .Padding(6)
                            .AlignRight()
                            .Text($"إجمالي الموظف: {group.TotalDiscount} | تلقائي: {group.TotalAutoDiscount} | يدوي: {group.TotalManualDiscount} | عدد المهام: {group.TasksCount}")
                            .Bold();


                    column.Item().PaddingBottom(10).LineHorizontal(0.5f);
                }
            });

            // ================= FOOTER =================
            page.Footer().AlignCenter()
                .Text(t =>
                {
                    t.Span("Task Manager System — صفحة ");
                    t.CurrentPageNumber();
                    t.Span(" من ");
                    t.TotalPages();
                });
                
        });
    }

    // ================= HELPERS =================

    void SummaryBox(RowDescriptor row, string title, decimal value)
    {
        row.RelativeItem().Border(1).Padding(6).AlignCenter().Column(c =>
        {
            c.Item().Text(title).FontSize(9);
            c.Item().Text(value.ToString("0.##")).Bold().FontSize(14);
        });
    }

    void SummaryBox(RowDescriptor row, string title, string value, string color)
    {
        row.RelativeItem()
           .Background(color)
           .Border(1)
           .BorderColor(Colors.Grey.Darken1)
           .Padding(8)
           .AlignCenter()
           .Column(c =>
           {
               c.Item().Text(title).FontSize(9).Bold();
               c.Item().Text(value).FontSize(16).Bold();
           });
    }


    void AddHeader(TableDescriptor table, string text)
    {
        table.Cell()
             .Border(1)
             .Background(Colors.Grey.Lighten2)
             .Padding(4)
             .AlignRight()
             .Text(text)
             .Bold();
    }


    void AddCell(TableDescriptor table, string text, string bg = null)
    {
        table.Cell()
             .Border(1)
             .Background(bg ?? Colors.White)
             .Padding(3)
             .AlignRight()
             .Text(text);
    }

}
