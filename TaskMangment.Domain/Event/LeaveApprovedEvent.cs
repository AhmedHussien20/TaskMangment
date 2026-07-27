using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class LeaveApprovedEvent
    {
        public int LeaveId { get; }
        public int EmployeeId { get; }
        public int ApprovedById { get; }
        public string EmployeeName { get; }
        public string ApprovedByName { get; }
        public string LeaveTypeName { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public LeaveApprovedEvent(int leaveId,int employeeId,int approvedById,string employeeName,string approvedByName, string leaveTypeName,DateTime startDate,DateTime endDate)
        {
            LeaveId = leaveId;
            EmployeeId = employeeId;
            ApprovedById = approvedById;
            EmployeeName = employeeName;
            ApprovedByName = approvedByName;
            LeaveTypeName = leaveTypeName;
            StartDate = startDate;
            EndDate = endDate;
        }
    }

}
