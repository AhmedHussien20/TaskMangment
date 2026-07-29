using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMangment.Domain.Entities
{
    /// <summary>
    /// Receiver role listens to Source role (e.g. BranchManager receives from Employee).
    /// Filtered at runtime by Role.NotificationScope (Branch / Area / Company).
    /// </summary>
    public class RoleNotificationSource : BaseEntity
    {
        public int RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        public int SourceRoleId { get; set; }
        [ForeignKey(nameof(SourceRoleId))]
        public Role? SourceRole { get; set; }
    }
}
