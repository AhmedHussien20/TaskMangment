using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class OfferAddEditDto
    {
        [Required, MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        public int? CourseId { get; set; }

        public int? SubjectId { get; set; }

        //public List<int>? AssignedStudentIds { get; set; } = new List<int>();
    }
    public class OfferAssignStudentsDto
    {
        public List<int> StudentIds { get; set; } = new();
    }

    public class OfferGetDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string CourseTitle { get; set; }
        public string SubjectTitle { get; set; }

        public List<string> AssignedStudents { get; set; } = new List<string>();
    }
}
