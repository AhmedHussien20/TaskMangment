using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Common.ApiRequests.NotificationRequest
{
    public class NotificationRequest
    {
        public ReferenceType ReferenceType { get; set; }
        public int ReferenceId { get; set; }

        public List<int> UserIds { get; set; } = new();

        public string TemplateKey { get; set; } = null!;
        public DateTime? ScheduledAt { get; set; }
    }

}
