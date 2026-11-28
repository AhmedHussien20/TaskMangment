using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    /// <summary>
    /// Adapter kept for existing call sites. Delegates to org-link based resolver (no RoleLevel).
    /// </summary>
    public class GetHigherManager : IGetHigherManager
    {
        private readonly IOrgManagerResolver _orgManagerResolver;
        private readonly IRepository<Employee> _employeeRepo;

        public GetHigherManager(
            IOrgManagerResolver orgManagerResolver,
            IRepository<Employee> employeeRepo)
        {
            _orgManagerResolver = orgManagerResolver;
            _employeeRepo = employeeRepo;
        }

        public async Task<List<int>> GetDirectHigherManagerIdsAsync(int currentEmployeeId)
        {
            var subjectActive = await _employeeRepo.GetAll(e =>
                    e.Id == currentEmployeeId && e.IsActive && !e.IsDeleted)
                .AnyAsync();
            if (!subjectActive)
                return new List<int>();

            var recipients = await _orgManagerResolver.GetEscalationRecipientsAsync(
                currentEmployeeId,
                NotificationEventKind.Escalation);

            if (recipients.Count > 0)
                return recipients.ToList();

            var operational = await _orgManagerResolver.GetOperationalManagersAsync(currentEmployeeId);
            if (operational.Count > 0)
                return operational.ToList();

            // Do not fall back to self — inactive/self loops are not notification targets.
            return new List<int>();
        }
    }
}
