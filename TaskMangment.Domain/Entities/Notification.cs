using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool IsRead { get; set; } = false;  
        public int ReferenceId { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
