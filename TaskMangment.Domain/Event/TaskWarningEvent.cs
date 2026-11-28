 
namespace TaskMangment.Domain.Event
{
    public class TaskWarningEvent
    {
        public int WarningId { get; }
        public int TaskId { get; }
        public string TaskTitle { get; set; }
        public string IssuedbyName { get; set; }
        public List<int> SendTo { get; set; }
        public string IssuedToName { get; set; }
        public string BranchName { get; set; }

        public TaskWarningEvent(
            int warningId,
            int taskId,
            string issuedbyName,
            List<int> sendTo,
            string issuedToName,
            string taskTitle,
            string? branchName = null)
        {
            WarningId = warningId;
            TaskId = taskId;
            IssuedbyName = issuedbyName;
            SendTo = sendTo;
            IssuedToName = issuedToName;
            TaskTitle = taskTitle;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
