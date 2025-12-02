using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class OfferAssignment:BaseEntity
    {
        [Required] public int OfferId { get; set; }
        [Required] public int StudentId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsAccepted { get; set; } = false;

        [ForeignKey(nameof(OfferId))] public Offer Offer { get; set; }
        [ForeignKey(nameof(StudentId))] public Student Student { get; set; }
    }
}
