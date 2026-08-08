namespace TaskMangment.Domain.Event
{
    public class LeaveEvent
    {
        public int LeaveId { get; }
        public int EmployeeId { get; }
        public string EmployeeName { get; }
        public string LeaveTypeName { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public string BranchName { get; }

        public LeaveEvent(
            int leaveId,
            int employeeId,
            string employeeName,
            string leaveTypeName,
            DateTime startDate,
            DateTime endDate,
            string? branchName = null)
        {
            LeaveId = leaveId;
            EmployeeId = employeeId;
            EmployeeName = employeeName;
            LeaveTypeName = leaveTypeName;
            StartDate = startDate;
            EndDate = endDate;
            BranchName = string.IsNullOrWhiteSpace(branchName) ? "-" : branchName;
        }
    }
}
