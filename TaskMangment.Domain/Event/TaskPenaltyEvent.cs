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
        public int IssuedtoId { get; set; }


        public TaskPenaltyEvent(int discountId, int taskId, string issuedbyName,int issuedtoId, string taskTitle)
        {
            DiscountId = discountId;
            TaskId = taskId;
            IssuedbyName = issuedbyName;
            IssuedtoId = issuedtoId;
            TaskTitle = taskTitle;
        }
    }
}
