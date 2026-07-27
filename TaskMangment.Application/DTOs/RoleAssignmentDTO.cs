using System.ComponentModel.DataAnnotations;

namespace TaskMangment.Application.DTOs
{
    public class AssignedEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string? BranchName { get; set; }
        public bool IsAssigned { get; set; }
        public string? RoleName { get; set; }
    }

    public class RoleWithManyEmployeeAssignDto
    {
        [Required]
        public List<EmployeeRoleAssignmentDto> Assignments { get; set; } = new();
    }

    public class EmployeeRoleAssignmentDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public bool Assign { get; set; }
    }

    public class ManagerBranchesDto
    {
        public int ManagerId { get; set; }
        public List<int> BranchIds { get; set; } = new();
    }

    public class GetManagerBranchesDto
    {
        public int ManagerId { get; set; }
        public int? EmployeeTypeId { get; set; }
        public List<int> BranchIds { get; set; } = new();
        public List<BranchLookupDto> branchLookupDtos { get; set; } = new();
        public bool RequiresBranchScope { get; set; }
        public bool RequiresEmployeeTypeScope { get; set; }
    }

    public class BranchLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class SetManagerBranchesRequest
    {
        /// <summary>Required when role has RequiresEmployeeTypeScope. Ignored for branch-only roles (defaults to Operations).</summary>
        public int? EmployeeTypeId { get; set; }
        public List<int>? BranchIds { get; set; } = new();
    }
}
