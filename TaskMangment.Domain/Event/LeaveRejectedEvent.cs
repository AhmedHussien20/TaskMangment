using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class LeaveRejectedEvent
    {
        public int LeaveId { get; }
        public int EmployeeId { get; }
        public string EmployeeName { get; }
        public string RejectedByName { get; }
        public string LeaveTypeName { get; }
        public string RejectReason { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public LeaveRejectedEvent(int leaveId,int employeeId,string employeeName,string rejectedByName,string leaveTypeName, string rejectReason,DateTime startDate, DateTime endDate)
        {
            LeaveId = leaveId;
            EmployeeId = employeeId;
            EmployeeName = employeeName;
            RejectedByName = rejectedByName;
            LeaveTypeName = leaveTypeName;
            RejectReason = rejectReason;
            StartDate = startDate;
            EndDate = endDate;
        }
    }

}
