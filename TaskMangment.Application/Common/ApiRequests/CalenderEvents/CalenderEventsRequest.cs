using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.CalenderEvents
{
    public class CalendarEventRequest : BaseApiRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
