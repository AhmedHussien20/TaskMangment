using System.Collections.Generic;

namespace TaskMangment.Domain.Event
{
    public class TaskPenaltyEvent
    {
        public int DiscountId { get; }
        public int TaskId { get; }
        public string TaskTitle { get; set; }
        public string IssuedbyName { get; set; }
        public List<int> SendTo { get; set; }
        public string IssuedToName { get; set; }
        public decimal Amount { get; }
        public string BranchName { get; set; }

        public TaskPenaltyEvent(
            int discountId,
            int taskId,
            string issuedbyName,
            List<int> sendTo,
            string issuedToName,
            string taskTitle,
            decimal amount,
            string? branchName = null)
        {
            DiscountId = discountId;
            TaskId = taskId;
            IssuedbyName = issuedbyName;
            SendTo = sendTo;
            IssuedToName = issuedToName;
            TaskTitle = taskTitle;
            Amount = amount;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
