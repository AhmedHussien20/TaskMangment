using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskRequestAddedEvent
    {
        public int RequestId { get; }
        public int TaskId { get; }
        public int CreatedByEmployeeId { get; }
        public List<int> Recipients { get; }

        public TaskRequestAddedEvent(int requestId, int taskId, int createdByEmployeeId, List<int> recipients)
        {
            RequestId = requestId;
            TaskId = taskId;
            CreatedByEmployeeId = createdByEmployeeId;
            Recipients = recipients;
        }
    }
}
