using System.Collections.Generic;

namespace TaskMangment.Domain.Event
{
    public class TaskCommentAddedEvent
    {
        public int CommentId { get; }
        public int TaskId { get; }
        public string taskTitle { get; set; }
        public string EmployeeName { get; set; }
        public List<int> Recipients { get; }
        public string BranchName { get; }

        public TaskCommentAddedEvent(
            int commentId,
            int taskId,
            string employeeName,
            List<int> recipients,
            string taskTitle,
            string? branchName = null)
        {
            CommentId = commentId;
            TaskId = taskId;
            EmployeeName = employeeName;
            Recipients = recipients;
            this.taskTitle = taskTitle;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
