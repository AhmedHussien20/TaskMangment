using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

namespace TaskMangment.API.Reports.Task
{
    public class TaskActivityPdfReport : IDocument
    {
        private readonly List<TaskActivityReportDto> _activities;

        public TaskActivityPdfReport(List<TaskActivityReportDto> activities)
        {
            _activities = activities;
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
                // Header
                page.Header().PaddingBottom(10).Column(column =>
                {
                    column.Item().AlignRight().Text("تقرير عن حركات المهام").FontSize(20).Bold();
                    column.Item().AlignRight().Text($"تاريخ التقرير: {DateTime.Now:yyyy/MM/dd}").FontSize(10).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // Content
                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); 
                        columns.RelativeColumn(3); 
                        columns.RelativeColumn(4); 
                        columns.RelativeColumn(2); 
                        columns.RelativeColumn(3);  
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("المهمة");
                        header.Cell().Element(HeaderCellStyle).Text("جهة التكليف");
                        header.Cell().Element(HeaderCellStyle).Text("التعليق");
                        header.Cell().Element(HeaderCellStyle).Text("تاريخ التعليق");
                        header.Cell().Element(HeaderCellStyle).Text("الموظف الذي علق");
                    });

                    int index = 0;
                    foreach (var activity in _activities)
                    {
                        string bgColor = index++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(activity.TaskTitleWithId);
                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(activity.AssignedBy);
                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(activity.Comment);
                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(activity.CommentDate.ToString("yyyy/MM/dd"));
                        table.Cell().Element(c => DataCellStyle(c, bgColor)).Text(activity.CommentedBy);
                    }
                });

                // Footer
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
            container.Border(1).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten2).Padding(6).AlignRight().DefaultTextStyle(x => x.Bold());

        static IContainer DataCellStyle(IContainer container, string backgroundColor) =>
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(6).AlignRight();
    }
}
