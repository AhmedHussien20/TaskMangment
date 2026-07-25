using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    /// <summary>
    /// Adapter kept for existing call sites. Delegates to org-link based resolver (no RoleLevel).
    /// </summary>
    public class GetHigherManager : IGetHigherManager
    {
        private readonly IOrgManagerResolver _orgManagerResolver;

        public GetHigherManager(IOrgManagerResolver orgManagerResolver)
        {
            _orgManagerResolver = orgManagerResolver;
        }

        public async Task<List<int>> GetDirectHigherManagerIdsAsync(int currentEmployeeId)
        {
            var recipients = await _orgManagerResolver.GetEscalationRecipientsAsync(
                currentEmployeeId,
                NotificationEventKind.Escalation);

            if (recipients.Count > 0)
                return recipients.ToList();

            var operational = await _orgManagerResolver.GetOperationalManagersAsync(currentEmployeeId);
            if (operational.Count > 0)
                return operational.ToList();

            return new List<int> { currentEmployeeId };
        }
    }
}
