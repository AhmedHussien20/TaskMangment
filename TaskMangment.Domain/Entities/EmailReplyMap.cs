using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Entities
{
    public class EmailReplyMap
    {
        public long Id { get; set; }
        public string Token { get; set; } = null!;
        public int TaskId { get; set; }
        public int OriginalCommentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
