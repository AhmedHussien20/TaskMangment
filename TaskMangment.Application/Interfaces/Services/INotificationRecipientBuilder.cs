using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    /// <summary>
    /// Builds notification recipients: task peers plus managers of a single anchor
    /// (actor who made the action, or subject who received it) — never fan-out from every peer.
    /// </summary>
    public interface INotificationRecipientBuilder
    {
        /// <param name="peerIds">Direct participants (assignees, AssignedBy, subject, …).</param>
        /// <param name="actorIdToExclude">Person who performed the action (excluded from recipients).</param>
        /// <param name="managerAnchorEmployeeId">Person whose RoleNotificationSource listeners are added.</param>
        Task<List<int>> BuildAsync(
            IEnumerable<int> peerIds,
            int? actorIdToExclude,
            int managerAnchorEmployeeId);
    }
}
