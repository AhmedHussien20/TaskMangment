using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Task
{
    public enum TaskDirection
    {
        Incoming = 1, 
        Outgoing = 2 
    }

    public class TaskRequest : BaseApiRequest
    {
        public TaskDirection? Direction { get; set; }    
        public int? TargetEmployeeId { get; set; }     
        public int? StatusId { get; set; }            

        public string? SearchKey { get; set; }         
        public int? PriorityId { get; set; }       

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public DateTime? DueFrom { get; set; }
        public DateTime? DueTo { get; set; }

        public List<int>? EmployeeIds { get; set; } 
    }

}
