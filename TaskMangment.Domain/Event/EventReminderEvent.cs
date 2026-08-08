using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public record EventReminderEvent(
    int UserId,
    string Message
);

}
