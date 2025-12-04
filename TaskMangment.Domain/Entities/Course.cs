using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Course : BaseEntity
    {
        [MaxLength(250)] 
        public string Title { get; set; }
        [MaxLength(1000)] 
        public string Description { get; set; }
        public ICollection<CourseSubject> Subjects { get; set; } = new List<CourseSubject>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
