using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class CourseAddEditDto
    {
        [Required, MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public List<string> Subjects { get; set; } = new List<string>();
    }

    public class CourseGetDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<string> Subjects { get; set; } = new List<string>();

        public int OfferCount { get; set; }
    }
}
