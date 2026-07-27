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
        public bool SeesAllTypesInBranchScope { get; set; }
        public bool HasManagerScope => BranchIds.Count > 0 || EmployeeTypeIds.Count > 0;
        public bool IsCompanyWide => Kind == AccessScopeKind.CompanyWide;
        public bool AllowsAllFunctionTypes => SeesAllTypesInBranchScope || EmployeeTypeIds.Count == 0;

        public UserAccessContext ToUserAccessContext() => new()
        {
            EmployeeId = EmployeeId,
            BranchIds = BranchIds,
            EmployeeTypeIds = EmployeeTypeIds,
            SeesAllTypesInBranchScope = SeesAllTypesInBranchScope
        };
    }
}
