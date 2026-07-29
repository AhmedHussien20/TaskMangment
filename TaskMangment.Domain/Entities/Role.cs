using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMangment.Domain.Entities
{
    public class Role : BaseEntity
    {
        public int? CompanyId { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        /// <summary>Optional legacy hierarchy. Not used for authorization; kept for future use.</summary>
        public int Level { get; set; } = 10;

        /// <summary>
        /// When true, assignee may be Area.ManagerEmployeeId.
        /// Branch coverage comes from Area (not ManagerBranches / نطاق التغطية).
        /// </summary>
        public bool RequiresBranchScope { get; set; }

        /// <summary>When true, assignees must configure one EmployeeType coverage.</summary>
        public bool RequiresEmployeeTypeScope { get; set; }

        /// <summary>
        /// Required when <see cref="RequiresEmployeeTypeScope"/> is true.
        /// All assignees cover this type; employee.EmployeeTypeId must match.
        /// </summary>
        public int? EmployeeTypeId { get; set; }
        [ForeignKey(nameof(EmployeeTypeId))]
        public EmployeeType? EmployeeType { get; set; }

        /// <summary>When true, assignee may be set as Branch.ManagerID.</summary>
        public bool CanBeBranchManager { get; set; }

        /// <summary>
        /// Geographic filter when this role receives notifications from NotifyFrom roles.
        /// </summary>
        public NotificationScope NotificationScope { get; set; } = NotificationScope.None;

        public Company Company { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
        public ICollection<RoleNotificationSource> NotificationSources { get; set; } = new List<RoleNotificationSource>();
    }
}
