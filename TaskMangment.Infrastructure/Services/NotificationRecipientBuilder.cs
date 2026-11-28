using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class NotificationRecipientBuilder : INotificationRecipientBuilder
    {
        private readonly IGetHigherManager _getHigherManager;
        private readonly IRepository<Employee> _employeeRepo;

        public NotificationRecipientBuilder(
            IGetHigherManager getHigherManager,
            IRepository<Employee> employeeRepo)
        {
            _getHigherManager = getHigherManager;
            _employeeRepo = employeeRepo;
        }

        public async Task<List<int>> BuildAsync(
            IEnumerable<int> peerIds,
            int? actorIdToExclude,
            int managerAnchorEmployeeId)
        {
            var recipients = peerIds?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();

            var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(managerAnchorEmployeeId);
            foreach (var id in managerIds)
            {
                if (!recipients.Contains(id))
                    recipients.Add(id);
            }

            if (actorIdToExclude.HasValue)
                recipients.Remove(actorIdToExclude.Value);

            if (recipients.Count == 0)
                return recipients;

            return await _employeeRepo
                .GetAll(e => recipients.Contains(e.Id) && e.IsActive && !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync();
        }
    }
}
