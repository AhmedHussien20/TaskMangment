using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    /// <summary>
    /// Notification recipients from each of the subject's matching listener roles (union).
    /// Scope is per role (Branch / Area / Company) — not hardcoded role names.
    /// All scopes: Operations (SeesAllTypes) hears all types; other types hear same type only.
    /// Branch scope: non-org listening-role holders in the subject's branch(es);
    /// org managers only via Branch.ManagerID / Area.ManagerEmployeeId (not home branch alone).
    /// Area scope: non-org holders whose home is in the subject's area(s);
    /// org managers only via Area.ManagerEmployeeId.
    /// </summary>
    public class OrgManagerResolver : IOrgManagerResolver
    {
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<RoleNotificationSource> _notifySourceRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<Area> _areaRepo;

        public OrgManagerResolver(
            IRepository<Employee> employeeRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<RoleNotificationSource> notifySourceRepo,
            IRepository<Branch> branchRepo,
            IRepository<Area> areaRepo)
        {
            _employeeRepo = employeeRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _notifySourceRepo = notifySourceRepo;
            _branchRepo = branchRepo;
            _areaRepo = areaRepo;
        }

        public async Task<IReadOnlyList<int>> GetOperationalManagersAsync(int employeeId)
            => await ResolveNotificationRecipientsAsync(employeeId);

        public async Task<IReadOnlyList<int>> GetEscalationRecipientsAsync(int actorId, NotificationEventKind eventKind)
        {
            _ = eventKind;
            return await ResolveNotificationRecipientsAsync(actorId);
        }

        public async Task<bool> IsOrgManagerOverAsync(int actorId, int targetEmployeeId)
        {
            if (actorId == targetEmployeeId)
                return false;

            var actor = await _employeeRepo.GetAll(e => e.Id == actorId && e.IsActive && !e.IsDeleted)
                .Select(e => new { e.BranchId })
                .FirstOrDefaultAsync();
            if (actor == null)
                return false;

            var targetActive = await _employeeRepo.GetAll(e => e.Id == targetEmployeeId && e.IsActive && !e.IsDeleted)
                .AnyAsync();
            if (!targetActive)
                return false;

            var subjectBranchIds = await GetSubjectBranchIdsAsync(actorId, actor.BranchId);
            if (subjectBranchIds.Count == 0)
                return false;

            var isBranchManager = await _branchRepo.GetAll(b =>
                    subjectBranchIds.Contains(b.Id) &&
                    !b.IsDeleted &&
                    b.ManagerID == targetEmployeeId)
                .AnyAsync();
            if (isBranchManager)
                return true;

            var areaIds = await _branchRepo.GetAll(b =>
                    subjectBranchIds.Contains(b.Id) &&
                    !b.IsDeleted &&
                    b.AreaId != null)
                .Select(b => b.AreaId!.Value)
                .Distinct()
                .ToListAsync();

            if (areaIds.Count == 0)
                return false;

            return await _areaRepo.GetAll(a =>
                    areaIds.Contains(a.Id) &&
                    !a.IsDeleted &&
                    a.ManagerEmployeeId == targetEmployeeId)
                .AnyAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<int>> GetListenableSubjectIdsAsync(
            int listenerEmployeeId,
            int? onlyListenerRoleId = null)
        {
            var listener = await _employeeRepo.GetAll(e => e.Id == listenerEmployeeId)
                .Select(e => new
                {
                    e.Id,
                    e.CompanyId,
                    e.BranchId,
                    e.EmployeeTypeId,
                    e.IsActive,
                    e.IsDeleted,
                    SeesAllTypes = e.EmployeeType != null &&
                        (e.EmployeeType.SeesAllTypesInBranchScope ||
                         e.EmployeeType.Code == EmployeeTypeCodes.Operations)
                })
                .FirstOrDefaultAsync();

            if (listener == null || listener.IsDeleted || !listener.IsActive)
                return Array.Empty<int>();

            var listeningLinks = await _employeeRoleRepo.GetAll(er =>
                    er.EmployeeId == listenerEmployeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted &&
                    er.Role.NotificationScope != NotificationScope.None &&
                    (onlyListenerRoleId == null || er.RoleId == onlyListenerRoleId.Value))
                .Select(er => new
                {
                    ListenerRoleId = er.RoleId,
                    Scope = er.Role!.NotificationScope
                })
                .Distinct()
                .ToListAsync();

            if (listeningLinks.Count == 0)
                return Array.Empty<int>();

            var listenerRoleIds = listeningLinks.Select(x => x.ListenerRoleId).Distinct().ToList();
            var sourceLinks = await _notifySourceRepo.GetAll(n =>
                    !n.IsDeleted &&
                    listenerRoleIds.Contains(n.RoleId))
                .Select(n => new { n.RoleId, n.SourceRoleId })
                .ToListAsync();

            if (sourceLinks.Count == 0)
                return Array.Empty<int>();

            var scopeByListenerRole = listeningLinks.ToDictionary(x => x.ListenerRoleId, x => x.Scope);
            var subjects = new HashSet<int>();

            var branchesManagedByListener = (await _branchRepo.GetAll(b =>
                    b.ManagerID == listenerEmployeeId && !b.IsDeleted && b.IsActive)
                .Select(b => b.Id)
                .ToListAsync()).ToHashSet();

            var areasManagedByListener = (await _areaRepo.GetAll(a =>
                    a.ManagerEmployeeId == listenerEmployeeId && !a.IsDeleted)
                .Select(a => a.Id)
                .ToListAsync()).ToHashSet();

            int? listenerAreaId = null;
            if (listener.BranchId.HasValue)
            {
                listenerAreaId = await _branchRepo.GetAll(b => b.Id == listener.BranchId.Value && !b.IsDeleted)
                    .Select(b => b.AreaId)
                    .FirstOrDefaultAsync();
            }

            foreach (var link in sourceLinks)
            {
                if (!scopeByListenerRole.TryGetValue(link.RoleId, out var scope) ||
                    scope == NotificationScope.None)
                    continue;

                var candidates = await _employeeRoleRepo.GetAll(er =>
                        er.RoleId == link.SourceRoleId &&
                        er.IsAssigned &&
                        !er.IsDeleted &&
                        er.Employee != null &&
                        er.Employee.IsActive &&
                        !er.Employee.IsDeleted &&
                        er.Employee.CompanyId == listener.CompanyId &&
                        er.EmployeeId != listenerEmployeeId)
                    .Select(er => new
                    {
                        er.EmployeeId,
                        er.Employee!.BranchId,
                        er.Employee.EmployeeTypeId
                    })
                    .Distinct()
                    .ToListAsync();

                if (candidates.Count == 0)
                    continue;

                switch (scope)
                {
                    case NotificationScope.Branch:
                        foreach (var c in candidates)
                        {
                            if (!HearsSubjectType(listener.SeesAllTypes, listener.EmployeeTypeId, c.EmployeeTypeId))
                                continue;

                            var subjectBranchIds = await GetSubjectBranchIdsAsync(c.EmployeeId, c.BranchId);
                            if (subjectBranchIds.Count == 0)
                                continue;

                            // Official coverage only for org managers (not "I live there").
                            if (subjectBranchIds.Any(branchesManagedByListener.Contains))
                            {
                                subjects.Add(c.EmployeeId);
                                continue;
                            }

                            if (areasManagedByListener.Count > 0)
                            {
                                var subjectAreaIds = await _branchRepo.GetAll(b =>
                                        subjectBranchIds.Contains(b.Id) &&
                                        !b.IsDeleted &&
                                        b.AreaId != null)
                                    .Select(b => b.AreaId!.Value)
                                    .Distinct()
                                    .ToListAsync();

                                if (subjectAreaIds.Any(areasManagedByListener.Contains))
                                {
                                    subjects.Add(c.EmployeeId);
                                    continue;
                                }
                            }

                            // Non-org listeners (e.g. مشرف تدريب): same home branch as subject geography.
                            var listenerIsOrgManager =
                                branchesManagedByListener.Count > 0 || areasManagedByListener.Count > 0;
                            if (!listenerIsOrgManager &&
                                listener.BranchId.HasValue &&
                                subjectBranchIds.Contains(listener.BranchId.Value))
                            {
                                subjects.Add(c.EmployeeId);
                            }
                        }
                        break;

                    case NotificationScope.Area:
                        foreach (var c in candidates)
                        {
                            if (!HearsSubjectType(listener.SeesAllTypes, listener.EmployeeTypeId, c.EmployeeTypeId))
                                continue;

                            var subjectBranchIds = await GetSubjectBranchIdsAsync(c.EmployeeId, c.BranchId);
                            if (subjectBranchIds.Count == 0)
                                continue;

                            var subjectAreaIds = await _branchRepo.GetAll(b =>
                                    subjectBranchIds.Contains(b.Id) &&
                                    !b.IsDeleted &&
                                    b.AreaId != null)
                                .Select(b => b.AreaId!.Value)
                                .Distinct()
                                .ToListAsync();

                            if (subjectAreaIds.Count == 0)
                                continue;

                            if (subjectAreaIds.Any(areasManagedByListener.Contains))
                            {
                                subjects.Add(c.EmployeeId);
                                continue;
                            }

                            var listenerIsOrgManager =
                                branchesManagedByListener.Count > 0 || areasManagedByListener.Count > 0;
                            if (!listenerIsOrgManager &&
                                listenerAreaId.HasValue &&
                                subjectAreaIds.Contains(listenerAreaId.Value))
                            {
                                subjects.Add(c.EmployeeId);
                            }
                        }
                        break;

                    case NotificationScope.Company:
                        foreach (var c in candidates)
                        {
                            if (HearsSubjectType(listener.SeesAllTypes, listener.EmployeeTypeId, c.EmployeeTypeId))
                                subjects.Add(c.EmployeeId);
                        }
                        break;
                }
            }

            return subjects.ToList();
        }

        private async Task<IReadOnlyList<int>> ResolveNotificationRecipientsAsync(int subjectEmployeeId)
        {
            var subject = await _employeeRepo.GetAll(e => e.Id == subjectEmployeeId)
                .Select(e => new { e.Id, e.CompanyId, e.BranchId, e.EmployeeTypeId, e.IsActive, e.IsDeleted })
                .FirstOrDefaultAsync();
            if (subject == null || subject.IsDeleted || !subject.IsActive)
                return Array.Empty<int>();

            var subjectRoleIds = await _employeeRoleRepo.GetAll(er =>
                    er.EmployeeId == subjectEmployeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted)
                .Select(er => er.RoleId)
                .Distinct()
                .ToListAsync();

            if (subjectRoleIds.Count == 0)
                return Array.Empty<int>();

            // Each listener role keeps its own scope + notify-from; results are unioned.
            var receiverRoles = await _notifySourceRepo.GetAll(n =>
                    !n.IsDeleted &&
                    subjectRoleIds.Contains(n.SourceRoleId) &&
                    n.Role != null &&
                    !n.Role.IsDeleted &&
                    n.Role.NotificationScope != NotificationScope.None)
                .Select(n => new
                {
                    ReceiverRoleId = n.RoleId,
                    Scope = n.Role!.NotificationScope
                })
                .Distinct()
                .ToListAsync();

            if (receiverRoles.Count == 0)
                return Array.Empty<int>();

            var subjectBranchIds = await GetSubjectBranchIdsAsync(subject.Id, subject.BranchId);
            var subjectAreaIds = await _branchRepo.GetAll(b =>
                    subjectBranchIds.Contains(b.Id) &&
                    !b.IsDeleted &&
                    b.AreaId != null)
                .Select(b => b.AreaId!.Value)
                .Distinct()
                .ToListAsync();

            var recipients = new HashSet<int>();

            foreach (var receiver in receiverRoles)
            {
                var candidates = await _employeeRoleRepo.GetAll(er =>
                        er.RoleId == receiver.ReceiverRoleId &&
                        er.IsAssigned &&
                        !er.IsDeleted &&
                        er.Employee != null &&
                        er.Employee.IsActive &&
                        !er.Employee.IsDeleted &&
                        er.Employee.CompanyId == subject.CompanyId &&
                        er.EmployeeId != subjectEmployeeId)
                    .Select(er => new
                    {
                        er.EmployeeId,
                        er.Employee!.BranchId,
                        er.Employee.EmployeeTypeId,
                        SeesAllTypes = er.Employee.EmployeeType != null &&
                            (er.Employee.EmployeeType.SeesAllTypesInBranchScope ||
                             er.Employee.EmployeeType.Code == EmployeeTypeCodes.Operations)
                    })
                    .Distinct()
                    .ToListAsync();

                if (candidates.Count == 0)
                    continue;

                switch (receiver.Scope)
                {
                    case NotificationScope.Branch:
                        // Non-org listening-role holders in the subject's branch(es),
                        // plus official Branch.ManagerID / Area.ManagerEmployeeId with the listening role.
                        // Org managers are NOT included merely because their home branch matches.
                        // Type rule: Operations hears all; other types hear same type only.
                        if (subjectBranchIds.Count == 0)
                            break;

                        var candidateIdsForBranch = candidates.Select(c => c.EmployeeId).ToHashSet();
                        var orgManagerCandidateIds = await GetOrgManagerEmployeeIdsAsync(candidateIdsForBranch);

                        foreach (var c in candidates)
                        {
                            if (!HearsSubjectType(c.SeesAllTypes, c.EmployeeTypeId, subject.EmployeeTypeId))
                                continue;

                            if (!c.BranchId.HasValue || !subjectBranchIds.Contains(c.BranchId.Value))
                                continue;

                            if (!orgManagerCandidateIds.Contains(c.EmployeeId))
                                recipients.Add(c.EmployeeId);
                        }

                        var branchManagers = await _branchRepo.GetAll(b =>
                                subjectBranchIds.Contains(b.Id) &&
                                !b.IsDeleted &&
                                b.IsActive &&
                                candidateIdsForBranch.Contains(b.ManagerID))
                            .Select(b => b.ManagerID)
                            .Distinct()
                            .ToListAsync();
                        foreach (var id in branchManagers)
                        {
                            var mgr = candidates.FirstOrDefault(c => c.EmployeeId == id);
                            if (mgr != null &&
                                HearsSubjectType(mgr.SeesAllTypes, mgr.EmployeeTypeId, subject.EmployeeTypeId))
                                recipients.Add(id);
                        }

                        if (subjectAreaIds.Count > 0)
                        {
                            var branchScopeAreaManagers = await _areaRepo.GetAll(a =>
                                    subjectAreaIds.Contains(a.Id) &&
                                    !a.IsDeleted &&
                                    a.ManagerEmployeeId != null &&
                                    candidateIdsForBranch.Contains(a.ManagerEmployeeId.Value))
                                .Select(a => a.ManagerEmployeeId!.Value)
                                .Distinct()
                                .ToListAsync();
                            foreach (var id in branchScopeAreaManagers)
                            {
                                var mgr = candidates.FirstOrDefault(c => c.EmployeeId == id);
                                if (mgr != null &&
                                    HearsSubjectType(mgr.SeesAllTypes, mgr.EmployeeTypeId, subject.EmployeeTypeId))
                                    recipients.Add(id);
                            }
                        }
                        break;

                    case NotificationScope.Area:
                        // Non-org listening-role holders whose home is in the subject's area(s),
                        // plus designated area managers holding the listening role.
                        // Type rule: Operations hears all; other types hear same type only.
                        if (subjectAreaIds.Count == 0)
                            break;

                        var areaBranchIds = await _branchRepo.GetAll(b =>
                                !b.IsDeleted &&
                                b.AreaId != null &&
                                subjectAreaIds.Contains(b.AreaId.Value))
                            .Select(b => b.Id)
                            .Distinct()
                            .ToListAsync();
                        var areaBranchIdSet = areaBranchIds.ToHashSet();

                        var candidateIdsForArea = candidates.Select(c => c.EmployeeId).ToHashSet();
                        var orgManagerIdsForArea = await GetOrgManagerEmployeeIdsAsync(candidateIdsForArea);

                        foreach (var c in candidates)
                        {
                            if (!HearsSubjectType(c.SeesAllTypes, c.EmployeeTypeId, subject.EmployeeTypeId))
                                continue;

                            if (!c.BranchId.HasValue || !areaBranchIdSet.Contains(c.BranchId.Value))
                                continue;

                            if (!orgManagerIdsForArea.Contains(c.EmployeeId))
                                recipients.Add(c.EmployeeId);
                        }

                        var areaManagers = await _areaRepo.GetAll(a =>
                                subjectAreaIds.Contains(a.Id) &&
                                !a.IsDeleted &&
                                a.ManagerEmployeeId != null &&
                                candidateIdsForArea.Contains(a.ManagerEmployeeId.Value))
                            .Select(a => a.ManagerEmployeeId!.Value)
                            .Distinct()
                            .ToListAsync();
                        foreach (var id in areaManagers)
                        {
                            var mgr = candidates.FirstOrDefault(c => c.EmployeeId == id);
                            if (mgr != null &&
                                HearsSubjectType(mgr.SeesAllTypes, mgr.EmployeeTypeId, subject.EmployeeTypeId))
                                recipients.Add(id);
                        }
                        break;

                    case NotificationScope.Company:
                        // Company-wide: Operations hears all types; other types hear same type only.
                        foreach (var c in candidates)
                        {
                            if (HearsSubjectType(c.SeesAllTypes, c.EmployeeTypeId, subject.EmployeeTypeId))
                                recipients.Add(c.EmployeeId);
                        }
                        break;
                }
            }

            return recipients.ToList();
        }

        /// <summary>
        /// Operations / SeesAllTypes hears every type; otherwise listener and subject must share type.
        /// </summary>
        private static bool HearsSubjectType(bool listenerSeesAllTypes, int listenerTypeId, int subjectTypeId)
            => listenerSeesAllTypes || listenerTypeId == subjectTypeId;

        private async Task<List<int>> GetSubjectBranchIdsAsync(int employeeId, int? homeBranchId)
        {
            var ids = new HashSet<int>();

            var managed = await _branchRepo.GetAll(b =>
                    b.ManagerID == employeeId && !b.IsDeleted && b.IsActive)
                .Select(b => b.Id)
                .ToListAsync();
            foreach (var id in managed)
                ids.Add(id);

            var isAreaManager = await _areaRepo.GetAll(a =>
                    a.ManagerEmployeeId == employeeId && !a.IsDeleted)
                .AnyAsync();

            // Org managers: home branch counts only if they are that branch's ManagerID.
            // Otherwise home would pull in a branch that already has another manager.
            if (homeBranchId.HasValue)
            {
                var isOrgManager = managed.Count > 0 || isAreaManager;
                if (!isOrgManager || managed.Contains(homeBranchId.Value))
                    ids.Add(homeBranchId.Value);
            }

            return ids.ToList();
        }

        private async Task<HashSet<int>> GetOrgManagerEmployeeIdsAsync(HashSet<int> employeeIds)
        {
            if (employeeIds.Count == 0)
                return new HashSet<int>();

            var branchMgrIds = await _branchRepo.GetAll(b =>
                    employeeIds.Contains(b.ManagerID) && !b.IsDeleted && b.IsActive)
                .Select(b => b.ManagerID)
                .Distinct()
                .ToListAsync();

            var areaMgrIds = await _areaRepo.GetAll(a =>
                    a.ManagerEmployeeId != null &&
                    employeeIds.Contains(a.ManagerEmployeeId.Value) &&
                    !a.IsDeleted)
                .Select(a => a.ManagerEmployeeId!.Value)
                .Distinct()
                .ToListAsync();

            return branchMgrIds.Concat(areaMgrIds).ToHashSet();
        }
    }
}
