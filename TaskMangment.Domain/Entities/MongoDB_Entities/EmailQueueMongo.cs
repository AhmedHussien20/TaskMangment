using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities.MongoDB_Entities
{
    public class EmailQueueMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string ToEmail { get; set; } = null!;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }

        public string TemplateKey { get; set; } = null!;
        public ReferenceType ReferenceType { get; set; }
        public int ReferenceId { get; set; }

        public DateTime ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }

        public EmailStatus Status { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
        public int? UserId { get; set; }

        public bool ForAll { get; set; } = false;
    }
}
