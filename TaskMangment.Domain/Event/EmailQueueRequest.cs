using System.Collections.Generic;
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
        /// <summary>Optional JSON object of extra template tokens (e.g. EmployeeName).</summary>
        public string? MetadataJson { get; set; }
        public bool ForAll { get; set; } = false;
    }
}
