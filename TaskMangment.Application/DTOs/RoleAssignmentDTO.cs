using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string?  RoleName { get; set; }
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
        public List<int> BranchIds { get; set; } = new();
        public List<BranchLookupDto> branchLookupDtos { get; set; } = new();
    }
    public class BranchLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }



}
