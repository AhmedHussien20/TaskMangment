using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.DTOs
{
    public class RoleAddEditDto
    {
        public int? CompanyId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool RequiresBranchScope { get; set; }
        public bool RequiresEmployeeTypeScope { get; set; }
        /// <summary>Required when RequiresEmployeeTypeScope is true — one type for the whole role.</summary>
        public int? EmployeeTypeId { get; set; }
        public bool CanBeBranchManager { get; set; }
    }

    public class RoleNotificationUpdateDto
    {
        public NotificationScope NotificationScope { get; set; } = NotificationScope.None;
        /// <summary>Role IDs this role receives notifications from.</summary>
        public List<int> NotifyFromRoleIds { get; set; } = new();
    }

    public class RoleGetDto
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public int EmployeeCount { get; set; }
        public int PermissionCount { get; set; }
        /// <summary>Distinct active employees in roles this role receives notifications from.</summary>
        public int NotifyFromEmployeeCount { get; set; }
        public bool RequiresBranchScope { get; set; }
        public bool RequiresEmployeeTypeScope { get; set; }
        public int? EmployeeTypeId { get; set; }
        public string? EmployeeTypeName { get; set; }
        public bool CanBeBranchManager { get; set; }
        public NotificationScope NotificationScope { get; set; }
        public List<int> NotifyFromRoleIds { get; set; } = new();
    }

    public class PermissionAddDto
    {
        [Required, MaxLength(100)]
        public string Code { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class PermissionGetDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }



    public class EmployeeRoleAssignDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public List<int> EmployeeIds { get; set; } = new();
    }
    public class UserRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public bool IsAssigned { get; set; }
    }

    //public class AssignedEmployeeDto
    //{
    //    public int EmployeeId { get; set; }
    //    public string FullName { get; set; }
    //    public string Email { get; set; }
    //    public string Mobile { get; set; }
    //    public string? BranchName { get; set; }
    //    public bool IsAssigned { get; set; }
    //}


}

public class AssignedPermissionDto
    {
        public int PermissionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsAssigned { get; set; }
    }

//public class RoleEmployeeBulkAssignDto
//{
//    [Required]
//    public int RoleId { get; set; }

//    [Required]
//    public List<EmployeeRoleAssignmentDto> Assignments { get; set; } = new();
//}

//public class EmployeeRoleAssignmentDto
//{
//    [Required]
//    public int EmployeeId { get; set; }

//    [Required]
//    public bool Assign { get; set; } // true = assign, false = unassign
//}




