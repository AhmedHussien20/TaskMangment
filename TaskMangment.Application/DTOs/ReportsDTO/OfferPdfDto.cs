using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs.ReportsDTO
{
    public class OfferPdfDto
    {
        public string CompanyHeader { get; set; } = "شركة الجذور الرقمية وفروعه";
        public string CompanyBranches { get; set; } = "عرعر - سكاكا - رفحاء - القريات - حفر الباطن";

        public string OfferTitle { get; set; } = "-";
        public string OfferDescription { get; set; } = "-";
        public string CourseName { get; set; } = "-";
        public string CourseSubject { get; set; } = "-";

        public string StartDate { get; set; } = "-";
        public string EndDate { get; set; } = "-";
        public string StudentName { get; set; } = "-";
        public string NationalId { get; set; } = "-";
        public string Mobile { get; set; } = "-";
        public string BranchName { get; set; } = "-";

        public string PaymentMethod { get; set; } = "-";
        public string Price { get; set; } = "-";
        public string InterestRate { get; set; } = "-";
        public string DiscountRate { get; set; } = "-";
        public string InstallmentValue { get; set; } = "-";
        public string NetAmount { get; set; } = "-";

        public string OfferOwner { get; set; } = "-";
        public string Specialization { get; set; } = "-";
        public string Notes { get; set; } = "-";
    }

    public class EmailAttachment
    {
        public string Name { get; set; } = default!;
        /// <summary>Base64 file content. Preferred for SMTP; also used by Brevo when set.</summary>
        public string? ContentBase64 { get; set; }
        /// <summary>Public absolute URL (e.g. blob + SAS). Used by Brevo when content is not set.</summary>
        public string? Url { get; set; }
    }
}
