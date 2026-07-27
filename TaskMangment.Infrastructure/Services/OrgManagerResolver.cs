using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    /// <summary>
    /// Notification / org-chain resolution using Branch.ManagerID, ManagerBranches, Area.ManagerEmployeeId,
    /// and RECEIVE_ORG_ESCALATIONS — never RoleLevel.
    /// </summary>
    public class OrgManagerResolver : IOrgManagerResolver
    {
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<EmployeeFunctionalScope> _functionalScopeRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<Area> _areaRepo;
        private readonly IEmployeePermissionService _permissions;

        public OrgManagerResolver(
            IRepository<Employee> employeeRepo,
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<EmployeeFunctionalScope> functionalScopeRepo,
            IRepository<Branch> branchRepo,
            IRepository<Area> areaRepo,
            IEmployeePermissionService permissions)
        {
            _employeeRepo = employeeRepo;
            _managerBranchesRepo = managerBranchesRepo;
            _functionalScopeRepo = functionalScopeRepo;
            _branchRepo = branchRepo;
            _areaRepo = areaRepo;
            _permissions = permissions;
        }

        public async Task<IReadOnlyList<int>> GetOperationalManagersAsync(int employeeId)
        {
            var employee = await _employeeRepo.GetAll(e => e.Id == employeeId)
                .Select(e => new { e.Id, e.CompanyId, e.BranchId, e.EmployeeTypeId })
                .FirstOrDefaultAsync();

            if (employee == null)
                return Array.Empty<int>();

            var recipients = new HashSet<int>();

            if (employee.BranchId.HasValue)
            {
                var branchManagerId = await _branchRepo.GetAll(b =>
                        b.Id == employee.BranchId.Value && !b.IsDeleted && b.ManagerID > 0)
                    .Select(b => b.ManagerID)
                    .FirstOrDefaultAsync();

                if (branchManagerId > 0 && branchManagerId != employeeId)
                {
                    if (await IsActiveEmployeeAsync(branchManagerId))
                        recipients.Add(branchManagerId);
                }

                var scopeManagers = await _managerBranchesRepo.GetAll(mb =>
                        mb.BranchId == employee.BranchId.Value &&
                        mb.IsActive &&
                        !mb.IsDeleted &&
                        mb.ManagerId != employeeId)
                    .Select(mb => mb.ManagerId)
                    .Distinct()
                    .ToListAsync();

                foreach (var managerId in scopeManagers)
                {
                    if (await MatchesFunctionalScopeAsync(managerId, employee.EmployeeTypeId) &&
                        await IsActiveEmployeeAsync(managerId))
                    {
                        recipients.Add(managerId);
                    }
                }
            }

            if (recipients.Count == 0)
            {
                var escalations = await GetCompanyEscalationHoldersAsync(employee.CompanyId, employeeId);
                foreach (var id in escalations)
                    recipients.Add(id);
            }

            return recipients.ToList();
        }

        public async Task<IReadOnlyList<int>> GetEscalationRecipientsAsync(int actorId, NotificationEventKind eventKind)
        {
            var actor = await _employeeRepo.GetAll(e => e.Id == actorId)
                .Select(e => new { e.Id, e.CompanyId, e.BranchId })
                .FirstOrDefaultAsync();

            if (actor == null)
                return Array.Empty<int>();

            var recipients = new HashSet<int>();

            var isBranchOwner = await IsBranchOwnerAsync(actorId);
            var isAreaOrMultiBranch = await IsAreaOrMultiBranchManagerAsync(actorId);

            if (eventKind == NotificationEventKind.EmployeeActivity ||
                (!isBranchOwner && !isAreaOrMultiBranch && eventKind == NotificationEventKind.Escalation))
            {
                foreach (var id in await GetOperationalManagersAsync(actorId))
                    recipients.Add(id);
            }

            if (isBranchOwner || eventKind == NotificationEventKind.BranchManagerActivity)
            {
                var areaManager = await FindAreaManagerForEmployeeAsync(actorId, actor.BranchId);
                if (areaManager.HasValue)
                    recipients.Add(areaManager.Value);

                foreach (var id in await GetCompanyEscalationHoldersAsync(actor.CompanyId, actorId))
                    recipients.Add(id);
            }

            if (isAreaOrMultiBranch || eventKind == NotificationEventKind.AreaManagerActivity)
            {
                foreach (var id in await GetCompanyEscalationHoldersAsync(actor.CompanyId, actorId))
                    recipients.Add(id);
            }

            recipients.Remove(actorId);
            return recipients.ToList();
        }

        public async Task<bool> IsOrgManagerOverAsync(int actorId, int targetEmployeeId)
        {
            if (actorId == targetEmployeeId)
                return false;

            var actor = await _employeeRepo.GetAll(e => e.Id == actorId)
                .Select(e => new { e.BranchId })
                .FirstOrDefaultAsync();

            if (actor == null)
                return false;

            // Target is Branch.ManagerID for actor's branch
            if (actor.BranchId.HasValue)
            {
                var isBranchManager = await _branchRepo.GetAll(b =>
                        b.Id == actor.BranchId.Value &&
                        !b.IsDeleted &&
                        b.ManagerID > 0 &&
                        b.ManagerID == targetEmployeeId)
                    .AnyAsync();

                if (isBranchManager)
                    return true;

                // Target has ManagerBranches covering actor's branch
                var coversBranch = await _managerBranchesRepo.GetAll(mb =>
                        mb.ManagerId == targetEmployeeId &&
                        mb.BranchId == actor.BranchId.Value &&
                        mb.IsActive &&
                        !mb.IsDeleted)
                    .AnyAsync();

                if (coversBranch)
                    return true;

                // Target is Area.ManagerEmployeeId for actor's area
                var areaId = await _branchRepo.GetAll(b => b.Id == actor.BranchId.Value && !b.IsDeleted)
                    .Select(b => b.AreaId)
                    .FirstOrDefaultAsync();

                if (areaId.HasValue)
                {
                    var isAreaManager = await _areaRepo.GetAll(a =>
                            a.Id == areaId.Value &&
                            !a.IsDeleted &&
                            a.ManagerEmployeeId == targetEmployeeId)
                        .AnyAsync();

                    if (isAreaManager)
                        return true;
                }
            }

            return false;
        }

        private async Task<bool> IsBranchOwnerAsync(int employeeId)
        {
            var ownsBranch = await _branchRepo.GetAll(b =>
                    !b.IsDeleted && b.ManagerID > 0 && b.ManagerID == employeeId)
                .AnyAsync();

            if (ownsBranch)
                return true;

            var managedCount = await _managerBranchesRepo.GetAll(mb =>
                    mb.ManagerId == employeeId && mb.IsActive && !mb.IsDeleted)
                .Select(mb => mb.BranchId)
                .Distinct()
                .CountAsync();

            return managedCount == 1;
        }

        private async Task<bool> IsAreaOrMultiBranchManagerAsync(int employeeId)
        {
            var isAreaManager = await _areaRepo.GetAll(a =>
                    !a.IsDeleted && a.ManagerEmployeeId == employeeId)
                .AnyAsync();

            if (isAreaManager)
                return true;

            var managedCount = await _managerBranchesRepo.GetAll(mb =>
                    mb.ManagerId == employeeId && mb.IsActive && !mb.IsDeleted)
                .Select(mb => mb.BranchId)
                .Distinct()
                .CountAsync();

            return managedCount > 1;
        }

        private async Task<int?> FindAreaManagerForEmployeeAsync(int employeeId, int? employeeBranchId)
        {
            int? branchId = employeeBranchId;
            if (!branchId.HasValue)
            {
                branchId = await _managerBranchesRepo.GetAll(mb =>
                        mb.ManagerId == employeeId && mb.IsActive && !mb.IsDeleted)
                    .Select(mb => (int?)mb.BranchId)
                    .FirstOrDefaultAsync();
            }

            if (!branchId.HasValue)
                return null;

            var areaId = await _branchRepo.GetAll(b => b.Id == branchId.Value && !b.IsDeleted)
                .Select(b => b.AreaId)
                .FirstOrDefaultAsync();

            if (!areaId.HasValue)
                return null;

            var areaManagerId = await _areaRepo.GetAll(a =>
                    a.Id == areaId.Value &&
                    !a.IsDeleted &&
                    a.ManagerEmployeeId != null &&
                    a.ManagerEmployeeId != employeeId)
                .Select(a => a.ManagerEmployeeId)
                .FirstOrDefaultAsync();

            if (!areaManagerId.HasValue)
                return null;

            return await IsActiveEmployeeAsync(areaManagerId.Value) ? areaManagerId : null;
        }

        private async Task<List<int>> GetCompanyEscalationHoldersAsync(int companyId, int excludeEmployeeId)
        {
            var candidates = await _employeeRepo.GetAll(e =>
                    e.CompanyId == companyId &&
                    e.IsActive &&
                    !e.IsDeleted &&
                    e.Id != excludeEmployeeId)
                .Select(e => e.Id)
                .ToListAsync();

            var result = new List<int>();
            foreach (var id in candidates)
            {
                if (await _permissions.HasAsync(id, PermissionCodes.ReceiveOrgEscalations) ||
                    await _permissions.HasAnyAsync(id, PermissionCodes.ViewCompanyTasks, PermissionCodes.ViewAllTasks))
                {
                    result.Add(id);
                }
            }

            return result;
        }

        private async Task<bool> MatchesFunctionalScopeAsync(int managerId, int employeeTypeId)
        {
            var scopes = await _functionalScopeRepo.GetAll(s =>
                    s.EmployeeId == managerId && !s.IsDeleted)
                .Select(s => new
                {
                    s.EmployeeTypeId,
                    SeesAll = s.EmployeeType != null &&
                              (s.EmployeeType.SeesAllTypesInBranchScope ||
                               s.EmployeeType.Code == EmployeeTypeCodes.Operations)
                })
                .ToListAsync();

            // No functional filter or Operations-like = all types in branch
            if (scopes.Count == 0 || scopes.Any(s => s.SeesAll))
                return true;

            return scopes.Any(s => s.EmployeeTypeId == employeeTypeId);
        }

        private Task<bool> IsActiveEmployeeAsync(int employeeId) =>
            _employeeRepo.GetAll(e => e.Id == employeeId && e.IsActive && !e.IsDeleted).AnyAsync();
    }
}
