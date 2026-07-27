namespace TaskMangment.Application.Common.Security
{
    public class UserAccessContext
    {
        public int EmployeeId { get; set; }
        public IReadOnlyList<int> BranchIds { get; set; } = [];
        /// <summary>EmployeeType Ids this manager covers (usually one).</summary>
        public IReadOnlyList<int> EmployeeTypeIds { get; set; } = [];
        /// <summary>True when coverage type is Operations-like: see all types within BranchIds.</summary>
        public bool SeesAllTypesInBranchScope { get; set; }
    }
}
