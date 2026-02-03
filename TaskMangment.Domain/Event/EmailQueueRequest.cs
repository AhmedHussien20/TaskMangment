using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Domain.Event
{
    public class EmailQueueRequest
    {
        public string TemplateKey { get; set; } = null!;
        public RecipientType RecipientType { get; set; }


        public ReferenceType ReferenceType { get; set; }
        public int ReferenceId { get; set; }

        public IEnumerable<int> UserIds { get; set; } = new List<int>();

        public DateTime? ScheduledAt { get; set; }
        //public string? MetadataJson { get; set; }  
        public bool ForAll { get; set; } = false;

    }

}
