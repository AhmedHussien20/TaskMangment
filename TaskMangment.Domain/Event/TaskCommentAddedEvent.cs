using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class TaskCommentAddedEvent
    {
        public int CommentId { get; }
        public int TaskId { get; }
        public string taskTitle { get; set; }
        public string EmployeeName { get; set; }
        public List<int> Recipients { get; }

        public TaskCommentAddedEvent(int commentId, int taskId, string employeeName, List<int> recipients, string taskTitle)
        {
            CommentId = commentId;
            TaskId = taskId;
            EmployeeName = employeeName;
            Recipients = recipients;
            this.taskTitle = taskTitle;
        }
    }

}
