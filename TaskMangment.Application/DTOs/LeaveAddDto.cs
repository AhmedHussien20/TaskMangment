using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class LeaveAddDto
    {
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }
    }
    public class LeaveGetDto
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }

        public LeaveStatus Status { get; set; }
        public string StatusName => Status.ToString();

        public string? RejectionReason { get; set; }
    }
    public class RejectLeaveDto
    {
        public string reason { get; set; }
    }

}
