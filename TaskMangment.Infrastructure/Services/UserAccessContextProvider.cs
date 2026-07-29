using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    /// <summary>
    /// Access branches from Branch.ManagerID and Area.ManagerEmployeeId (not ManagerBranches).
    /// Employee type scope still from EmployeeFunctionalScope.
    /// </summary>
    public class UserAccessContextProvider : IUserAccessContextProvider
    {
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<Area> _areaRepo;
        private readonly IRepository<EmployeeFunctionalScope> _employeeFunctionalScopeRepo;

        public UserAccessContextProvider(
            IRepository<Branch> branchRepo,
            IRepository<Area> areaRepo,
            IRepository<EmployeeFunctionalScope> employeeFunctionalScopeRepo)
        {
            _branchRepo = branchRepo;
            _areaRepo = areaRepo;
            _employeeFunctionalScopeRepo = employeeFunctionalScopeRepo;
        }

        public async Task<UserAccessContext> GetAsync(int employeeId)
        {
            var asBranchManager = await _branchRepo.GetAll(b =>
                    b.ManagerID == employeeId &&
                    !b.IsDeleted &&
                    b.IsActive)
                .Select(b => b.Id)
                .ToListAsync();

            var managedAreaIds = await _areaRepo.GetAll(a =>
                    a.ManagerEmployeeId == employeeId &&
                    !a.IsDeleted)
                .Select(a => a.Id)
                .ToListAsync();

            var asAreaManager = managedAreaIds.Count == 0
                ? new List<int>()
                : await _branchRepo.GetAll(b =>
                        b.AreaId != null &&
                        managedAreaIds.Contains(b.AreaId.Value) &&
                        !b.IsDeleted &&
                        b.IsActive)
                    .Select(b => b.Id)
                    .ToListAsync();

            var branchIds = asBranchManager.Concat(asAreaManager).Distinct().ToList();

            var typeRows = await _employeeFunctionalScopeRepo.GetAll()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .Select(x => new
                {
                    x.EmployeeTypeId,
                    SeesAll = x.EmployeeType != null && x.EmployeeType.SeesAllTypesInBranchScope
                })
                .ToListAsync();

            return new UserAccessContext
            {
                EmployeeId = employeeId,
                BranchIds = branchIds,
                EmployeeTypeIds = typeRows.Select(x => x.EmployeeTypeId).Distinct().ToList(),
                SeesAllTypesInBranchScope = typeRows.Any(x => x.SeesAll)
            };
        }
    }
}
