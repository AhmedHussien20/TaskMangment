using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities.Enum;

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

        public CalendarEventType EventType { get; set; } = CalendarEventType.Reminder;
        public bool Public { get; set; }


        public int reminder { get; set; }

    }
    public class CalendarEventGetDto
    {

        public int Id { get; set; } 

        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AllDay { get; set; }

        public int? RelatedTaskId { get; set; }
        public string RelatedTaskTitle { get; set; }

        public int? CompanyId { get; set; }

        public CalendarEventType EventType { get; set; }

        public string EventTypeText => EventType.ToString();

        public int reminder { get; set; }
        public bool Public { get; set; }

        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }



    }
}
