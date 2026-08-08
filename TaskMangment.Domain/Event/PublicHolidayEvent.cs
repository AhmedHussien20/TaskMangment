using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class PublicHolidayEvent
    {
        public int CalenderId { get; }
        public string HolidayName { get; }
        public DateTime HolidayDate { get; }

        public PublicHolidayEvent(int calenderId, string holidayName, DateTime holidayDate)
        {
            CalenderId = calenderId;
            HolidayName = holidayName;
            HolidayDate = holidayDate;
        }
    }
}
