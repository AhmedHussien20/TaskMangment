using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class CourseSubject : BaseEntity
    {
        public int SubjectId { get; set; }
        [Required] public int CourseId { get; set; }
        [MaxLength(250)] public string Title { get; set; }

        [ForeignKey(nameof(CourseId))] public Course Course { get; set; }
        public ICollection<Offer> Offers { get; set; }
    }
}
