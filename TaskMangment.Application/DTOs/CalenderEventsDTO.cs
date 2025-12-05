using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.DTOs
{
    public class CalendarEventAddEditDto
    {
        [Required, MaxLength(300)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AllDay { get; set; } = false;

        public int? RelatedTaskId { get; set; }
    }
    public class CalendarEventGetDto
    {

        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AllDay { get; set; }

        public int? RelatedTaskId { get; set; }
        public string RelatedTaskTitle { get; set; }

        public int? CompanyId { get; set; }
    }
}
