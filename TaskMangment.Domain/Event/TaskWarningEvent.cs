 
namespace TaskMangment.Domain.Event
{
    public class TaskWarningEvent
    {
        public int WarningId { get; }
        public int TaskId { get; }
        public string TaskTitle { get; set; }
        public string IssuedbyName { get; set; }
        public int IssuedtoId { get; set; }


        public TaskWarningEvent(int warningId, int taskId, string issuedbyName, int issuedtoId, string taskTitle)
        {
            WarningId = warningId;
            TaskId = taskId;
            IssuedbyName = issuedbyName;
            IssuedtoId = issuedtoId;
            TaskTitle = taskTitle;
        }
    }
}
