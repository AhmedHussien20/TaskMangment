using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IAccessScopeResolver
    {
        Task<ResolvedAccessScope> ResolveAsync(int employeeId);

        IQueryable<Employee> FilterEmployees(
            IQueryable<Employee> query,
            ResolvedAccessScope scope,
            AccessIntent intent = AccessIntent.View);

        Task<bool> CanAssignAsync(int actorId, int targetEmployeeId);

        Task<bool> CanViewEmployeeAsync(int actorId, int targetEmployeeId);
    }
}
