using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Common.ApiRequests.Notification
{
    public class NotificationCommand
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public List<NotificationChannel> Channels { get; set; }
    }

}
