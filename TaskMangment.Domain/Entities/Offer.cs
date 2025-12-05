using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Offer : BaseEntity
    {
        [MaxLength(250)] public string Title { get; set; }
        [MaxLength(1000)] public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CourseId { get; set; }
        public int? SubjectId { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CourseId))] public Course Course { get; set; }
        [ForeignKey(nameof(SubjectId))] public CourseSubject Subject { get; set; }
        public ICollection<OfferAssignment> Assignments { get; set; } = new List<OfferAssignment>();
    }
}
