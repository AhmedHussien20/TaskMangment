using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.PDF;

namespace TaskMangment.Infrastructure.Services
{
    public class OfferSendService: IOfferSendService
    {
        private readonly IRepository<Offer> _offerRepo;

        public OfferSendService(IRepository<Offer> offerRepo)
        {
            _offerRepo = offerRepo;
        }

        public async Task<byte[]> GenerateOfferPdfBytesAsync(int offerId)
        {
            var offer = await _offerRepo.GetAll(o => o.Id == offerId)
                .Include(o => o.Course)
                .Include(o => o.Subject)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (offer == null)
                throw new Exception("Offer not found");

            Dictionary<string, string>? body = null;

            if (!string.IsNullOrWhiteSpace(offer.Body))
            {
                body = JsonSerializer.Deserialize<Dictionary<string, string>>(offer.Body);
            }

            var dto = new OfferPdfDto
            {
                OfferTitle = offer.Title ?? "-",
                OfferDescription = offer.Description ?? "-",
                CourseName = offer.Course?.Title ?? "-",
                CourseSubject = offer.Subject?.Title ?? "-",
                StartDate = offer.StartDate?.ToString("yyyy-MM-dd") ?? "-",
                EndDate = offer.EndDate?.ToString("yyyy-MM-dd") ?? "-",
                PaymentMethod = body?.GetValueOrDefault("PaymentMethod") ?? "-",
                Price = body?.GetValueOrDefault("Price") ?? "-",
                InterestRate = body?.GetValueOrDefault("InterestRate") ?? "-",
                DiscountRate = body?.GetValueOrDefault("DiscountRate") ?? "-",
                InstallmentValue = body?.GetValueOrDefault("InstallmentValue") ?? "-",
                NetAmount = body?.GetValueOrDefault("NetAmount") ?? "-",
                OfferOwner = body?.GetValueOrDefault("OfferOwner") ?? "-",
                Specialization = body?.GetValueOrDefault("Specialization") ?? "-"
            };

            var doc = new OfferPdfDocument(dto);
            return doc.GeneratePdf();
        }

    }
}
