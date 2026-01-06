using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities.Enum
{
    public enum CalendarEventType
    {
        Meeting = 1,

        Appointment = 2,

        TaskDeadline = 3,

        Holiday = 4,

        Reminder = 5,

        Birthday = 6,

        Anniversar = 7,

        Comment = 8,
    }
}
