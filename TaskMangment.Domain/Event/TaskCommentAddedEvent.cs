using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskRequstAddedEvent
    {
        public int CommentId { get; }
        public int TaskId { get; }
        public int CreatedByEmployeeId { get; }
        public List<int> Recipients { get; }

        public TaskRequstAddedEvent(int commentId, int taskId, int createdByEmployeeId, List<int> recipients)
        {
            CommentId = commentId;
            TaskId = taskId;
            CreatedByEmployeeId = createdByEmployeeId;
            Recipients = recipients;
        }
    }

}
