using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class EmployeeTaskCommentsPdfReport : IDocument
    {
        private readonly List<EmployeeTaskCommentRowDto> _data;

        public EmployeeTaskCommentsPdfReport(List<EmployeeTaskCommentRowDto> data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontFamily("Cairo").FontSize(11));
                page.ContentFromRightToLeft();

                page.Header().PaddingBottom(10).Column(column =>
                {
                    column.Item().AlignRight().Text("تقرير تعليقات الموظف على المهمة").FontSize(18).Bold();
                    column.Item().AlignRight().Text($"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}").FontSize(10).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); // rank
                        columns.RelativeColumn(2);  // date
                        columns.RelativeColumn(6);  // comment
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("ترتيب");
                        header.Cell().Element(HeaderCellStyle).Text("تاريخ التعليق");
                        header.Cell().Element(HeaderCellStyle).Text("التعليق");
                    });

                    int index = 0;
                    int rank = 1;
                    foreach (var item in _data)
                    {
                        string bgColor = index++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(rank++.ToString());
                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(item.CommentDate.ToString("yyyy/MM/dd"));
                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(item.CommentText ?? "-");
                    }
                });

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
            container.Border(1)
                .BorderColor(Colors.Grey.Darken1)
                .Background(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignRight()
                .DefaultTextStyle(x => x.Bold());

        static IContainer DataCellStyle(IContainer container, string backgroundColor) =>
            container.Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(backgroundColor)
                .Padding(6)
                .AlignRight();
    }
}

