namespace TaskMangment.Application.Interfaces.Services
{
    /// <summary>
    /// Single source of truth for employee capability checks (union of permissions across assigned roles).
    /// </summary>
    public interface IEmployeePermissionService
    {
        Task<bool> HasAsync(int employeeId, string permissionCode);
        Task<bool> HasAnyAsync(int employeeId, params string[] permissionCodes);
        Task<IReadOnlySet<string>> GetPermissionsAsync(int employeeId);
        /// <summary>True when the employee has at least one assigned, non-deleted role.</summary>
        Task<bool> HasActiveRoleAsync(int employeeId);
    }
}
