namespace TaskMangment.Domain.Event
{
    public class LeaveRejectedEvent
    {
        public int LeaveId { get; }
        public int EmployeeId { get; }
        public int RejectedById { get; }
        public string EmployeeName { get; }
        public string RejectedByName { get; }
        public string LeaveTypeName { get; }
        public string RejectReason { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public string BranchName { get; }

        public LeaveRejectedEvent(
            int leaveId,
            int employeeId,
            int rejectedById,
            string employeeName,
            string rejectedByName,
            string leaveTypeName,
            string rejectReason,
            DateTime startDate,
            DateTime endDate,
            string? branchName = null)
        {
            LeaveId = leaveId;
            EmployeeId = employeeId;
            RejectedById = rejectedById;
            EmployeeName = employeeName;
            RejectedByName = rejectedByName;
            LeaveTypeName = leaveTypeName;
            RejectReason = rejectReason;
            StartDate = startDate;
            EndDate = endDate;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
