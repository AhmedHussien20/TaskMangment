namespace TaskMangment.Application.Common.Security
{
    public enum AccessScopeKind
    {
        SelfOnly = 0,
        OwnBranch = 1,
        ManagerScoped = 2,
        CompanyWide = 3
    }

    public enum AccessIntent
    {
        View = 0,
        Assign = 1
    }

    /// <summary>
    /// Resolved data reach for an employee. Built from permissions + ManagerBranches + functional scopes.
    /// </summary>
    public class ResolvedAccessScope
    {
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }
        public int? OwnBranchId { get; set; }
        public AccessScopeKind Kind { get; set; }
        public IReadOnlyList<int> BranchIds { get; set; } = [];
        public IReadOnlyList<int> EmployeeTypeIds { get; set; } = [];
        /// <summary>Types that must stay within actor branch(es); from roles with type+branch flags.</summary>
        public IReadOnlyList<int> BranchRestrictedEmployeeTypeIds { get; set; } = [];
        public bool SeesAllTypesInBranchScope { get; set; }
        public bool HasManagerScope => BranchIds.Count > 0 || EmployeeTypeIds.Count > 0;
        public bool IsCompanyWide => Kind == AccessScopeKind.CompanyWide;
        public bool AllowsAllFunctionTypes => SeesAllTypesInBranchScope || EmployeeTypeIds.Count == 0;

        /// <summary>
        /// Employees hidden from View lists/360 for ManagerScoped/OwnBranch actors:
        /// org superiors (area/branch managers above them) and company-wide users.
        /// </summary>
        public IReadOnlyList<int> ViewExcludeEmployeeIds { get; set; } = [];

        public UserAccessContext ToUserAccessContext() => new()
        {
            EmployeeId = EmployeeId,
            OwnBranchId = OwnBranchId,
            BranchIds = BranchIds,
            EmployeeTypeIds = EmployeeTypeIds,
            BranchRestrictedEmployeeTypeIds = BranchRestrictedEmployeeTypeIds,
            SeesAllTypesInBranchScope = SeesAllTypesInBranchScope
        };
    }
}
