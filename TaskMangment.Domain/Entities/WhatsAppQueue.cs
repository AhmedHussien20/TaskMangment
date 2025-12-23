using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class WhatsAppQueue
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Phone { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime ScheduledAt { get; set; }
        public int RetryCount { get; set; }
    }

}
