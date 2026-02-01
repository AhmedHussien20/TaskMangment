using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskMangment.Application.DTOs.ReportsDTO;

public class OfferPdfDocument : IDocument
{
    private readonly OfferPdfDto _data;

    public OfferPdfDocument(OfferPdfDto data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontFamily("Cairo").FontSize(11));
            page.ContentFromRightToLeft();

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text(_data.CompanyHeader).Bold().FontSize(12);
                col.Item().AlignCenter().Text(_data.CompanyBranches).FontSize(10).FontColor(Colors.Grey.Darken2);

                col.Item().PaddingTop(8).AlignCenter().Text("عرض سعر").Bold().FontSize(16);
                col.Item().PaddingTop(6).LineHorizontal(1);
            });

            page.Content().PaddingTop(12).Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2.2f); 
                        c.RelativeColumn(3.8f);
                    });

                    Row(table, "عنوان العـــــرض", _data.OfferTitle);
                    Row(table, "تاريــخ البدء", _data.StartDate);
                    Row(table, "اسم العميـــــــل", _data.StudentName);
                    Row(table, "رقم الهوية / الاقامة", _data.NationalId);
                    Row(table, "رقم الجــــــــــوال", _data.Mobile);
                    Row(table, "الفــــــــــــــــــرع", _data.BranchName);
                    Row(table, "طــريقة الدفـــــــع", _data.PaymentMethod);
                    Row(table, "السعــر المطلـــــوب", _data.Price);
                    Row(table, "نسبــــــة الفـــائدة %", _data.InterestRate);
                    Row(table, "نسبــــــة الخصــــــم %", _data.DiscountRate);
                    Row(table, "قيمة القســــــــــط", _data.InstallmentValue);
                    Row(table, "صافى المبلغ المطلوب", _data.NetAmount);
                    Row(table, "اسم مقدم العرض", _data.OfferOwner);
                    Row(table, "التخصص المطلوب", _data.Specialization);
                    Row(table, "ملاحظــــــــــات", _data.Notes);
                });

                col.Item().PaddingTop(10);

                col.Item()
                    .Border(1)
                    .BorderColor(Colors.Red.Darken1)
                    .Background(Colors.Red.Lighten5)
                    .Padding(8)
                    .AlignCenter()
                    .Text("هذا العرض ساري لمدة أسبوع واحد فقط من تاريخه")
                    .Bold()
                    .FontColor(Colors.Red.Darken2);

                col.Item().PaddingTop(20);

                col.Item().Row(r =>
                {
                    r.RelativeItem().AlignCenter().Text("توقيع مدير المعهد");
                    r.RelativeItem().AlignCenter().Text("توقيع مقدم العرض");
                    r.RelativeItem().AlignCenter().Text("توقيع العميل");
                });
            });

            page.Footer().AlignCenter()
    .Text(t =>
    {
        t.Span("Printing Date: ");
        t.Span(DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
        t.Span("    Page ");
        t.CurrentPageNumber();
        t.Span(" of ");
        t.TotalPages();
    });
    

        });
    }

    private void Row(TableDescriptor table, string label, string value)
    {
        table.Cell()
            .Border(1)
            .Background(Colors.Grey.Lighten3)
            .Padding(6)
            .AlignRight()
            .Text(label).Bold();

        table.Cell()
            .Border(1)
            .Padding(6)
            .AlignRight()
            .Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
    }
}


