using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public TaskPenaltyEvent(int discountId, int taskId, string issuedbyName, List<int> sendTo, string issuedToName, string taskTitle)
        {
            DiscountId = discountId;
            TaskId = taskId;
            IssuedbyName = issuedbyName;
            SendTo = sendTo;
            IssuedToName = issuedToName;
            TaskTitle = taskTitle;
        }
    }
}
