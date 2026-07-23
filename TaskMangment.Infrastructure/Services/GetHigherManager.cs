using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class GetHigherManager : IGetHigherManager
    {
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<EmployeeFunctionalScope> _employeeFunctionScopeRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<Area> _areaRepo;

        public GetHigherManager(
            IRepository<Employee> employeeRepo,
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<EmployeeFunctionalScope> employeeFunctionScopeRepo,
            IRepository<Branch> branchRepo,
            IRepository<Area> areaRepo)
        {
            _employeeRepo = employeeRepo;
            _managerBranchesRepo = managerBranchesRepo;
            _employeeFunctionScopeRepo = employeeFunctionScopeRepo;
            _branchRepo = branchRepo;
            _areaRepo = areaRepo;
        }

        public async Task<List<int>> GetDirectHigherManagerIdsAsync(int currentEmployeeId)
        {
            var currentUser = await _employeeRepo.GetAll(e => e.Id == currentEmployeeId)
                .Select(e => new
                {
                    e.Id,
                    e.CompanyId,
                    e.BranchId,
                    e.FunctionCode,
                    Level = e.EmployeeRoles
                               .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
                               .Select(er => (int?)er.Role.Level)
                               .Max() ?? (int)RoleLevelEnum.Employee
                })
                .FirstOrDefaultAsync();

            if (currentUser == null)
            {
                return new List<int> { currentEmployeeId };
            }

            // Level 80 (BranchesManager) → company Admin (100)
            if (currentUser.Level == (int)RoleLevelEnum.BranchesManager)
            {
                var adminId = await FindAdminInCompanyAsync(currentUser.CompanyId, currentEmployeeId);
                if (adminId.HasValue)
                    return new List<int> { adminId.Value };
            }

            // Level 70 (Manager) → Area manager (80)
            if (currentUser.Level == (int)RoleLevelEnum.Manager)
            {
                var areaManagerId = await FindAreaManagerAsync(
                    currentUser.Id,
                    currentUser.BranchId,
                    currentEmployeeId);

                if (areaManagerId.HasValue)
                    return new List<int> { areaManagerId.Value };
            }

            int? higherManagerId = null;

            if (currentUser.FunctionCode == FunctionCode.Operations)
            {
                higherManagerId = await FindHigherManagerInBranchAsync(
                    currentUser.Id,
                    currentUser.CompanyId,
                    currentUser.BranchId,
                    currentUser.Level);
            }
            else
            {
                higherManagerId = await FindHigherManagerByFunctionAsync(
                    currentUser.Id,
                    currentUser.CompanyId,
                    currentUser.FunctionCode,
                    currentUser.Level);
            }

            if (higherManagerId.HasValue)
            {
                return new List<int> { higherManagerId.Value };
            }

            var topEmployeesInCompany = await FindTopLevelEmployeeAsync(currentUser.CompanyId);

            return topEmployeesInCompany.Count > 0
                ? topEmployeesInCompany
                : new List<int> { currentEmployeeId };
        }

        private async Task<int?> FindAreaManagerAsync(
            int currentEmployeeId,
            int? employeeBranchId,
            int excludeEmployeeId)
        {
            int? branchId = employeeBranchId;

            if (!branchId.HasValue)
            {
                branchId = await _managerBranchesRepo.GetAll(mb =>
                        mb.ManagerId == currentEmployeeId &&
                        mb.IsActive &&
                        !mb.IsDeleted)
                    .Select(mb => (int?)mb.BranchId)
                    .FirstOrDefaultAsync();
            }

            if (!branchId.HasValue)
                return null;

            var areaId = await _branchRepo.GetAll(b =>
                    b.Id == branchId.Value &&
                    !b.IsDeleted)
                .Select(b => b.AreaId)
                .FirstOrDefaultAsync();

            if (!areaId.HasValue)
                return null;

            var areaManagerId = await _areaRepo.GetAll(a =>
                    a.Id == areaId.Value &&
                    !a.IsDeleted &&
                    a.ManagerEmployeeId != null &&
                    a.ManagerEmployeeId != excludeEmployeeId)
                .Select(a => a.ManagerEmployeeId)
                .FirstOrDefaultAsync();

            if (!areaManagerId.HasValue)
                return null;

            var isActive = await _employeeRepo.GetAll(e =>
                    e.Id == areaManagerId.Value &&
                    e.IsActive &&
                    !e.IsDeleted)
                .AnyAsync();

            return isActive ? areaManagerId : null;
        }

        private async Task<int?> FindAdminInCompanyAsync(int companyId, int currentEmployeeId)
        {
            return await _employeeRepo.GetAll(e =>
                    e.CompanyId == companyId &&
                    e.IsActive &&
                    e.Id != currentEmployeeId &&
                    e.EmployeeRoles.Any(er =>
                        er.IsAssigned && !er.IsDeleted &&
                        er.Role != null && er.Role.Level == (int)RoleLevelEnum.Admin))
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<int?> FindHigherManagerInBranchAsync(int currentEmployeeId, int companyId, int? employeeBranchId, int currentLevel)
        {
            var managedBranchIds = await _managerBranchesRepo.GetAll(mb =>
                    mb.ManagerId == currentEmployeeId && mb.IsActive && !mb.IsDeleted)
                .Select(mb => mb.BranchId)
                .Distinct()
                .ToListAsync();

            int? searchBranchId = null;
            if (managedBranchIds.Count == 1)
            {
                searchBranchId = managedBranchIds[0];
            }
            else if (managedBranchIds.Count == 0 && employeeBranchId.HasValue)
            {
                searchBranchId = employeeBranchId.Value;
            }
            else
            {
                return null;
            }

            var managersInSameBranch = _managerBranchesRepo.GetAll(mb =>
                mb.BranchId == searchBranchId.Value &&
                mb.IsActive &&
                !mb.IsDeleted &&
                mb.ManagerId != currentEmployeeId)
                .Select(mb => mb.ManagerId);

            return await _employeeRepo.GetAll(e =>
                    e.CompanyId == companyId &&
                    e.IsActive &&
                    e.FunctionCode == FunctionCode.Operations &&
                    managersInSameBranch.Contains(e.Id))
                .Select(e => new
                {
                    EmployeeId = e.Id,
                    MaxLevel = e.EmployeeRoles.Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null).Select(er => (int?)er.Role.Level).Max() ?? (int)RoleLevelEnum.Employee
                })
                .Where(e => e.MaxLevel > currentLevel)
                .OrderBy(e => e.MaxLevel)
                .Select(e => (int?)e.EmployeeId)
                .FirstOrDefaultAsync();
        }

        private async Task<int?> FindHigherManagerByFunctionAsync(int currentEmployeeId, int companyId, FunctionCode functionCode, int currentLevel)
        {
            var employeesWithSameFunction = _employeeFunctionScopeRepo.GetAll(efs => efs.FunctionCode == functionCode)
                .Select(efs => efs.EmployeeId);

            return await _employeeRepo.GetAll(e =>
                    e.CompanyId == companyId &&
                    e.IsActive &&
                    e.Id != currentEmployeeId &&
                    employeesWithSameFunction.Contains(e.Id))
                .Select(e => new
                {
                    EmployeeId = e.Id,
                    MaxLevel = e.EmployeeRoles.Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null).Select(er => (int?)er.Role.Level).Max() ?? (int)RoleLevelEnum.Employee
                })
                .Where(e => e.MaxLevel > currentLevel)
                .OrderBy(e => e.MaxLevel)
                .Select(e => (int?)e.EmployeeId)
                .FirstOrDefaultAsync();
        }

        private async Task<List<int>> FindTopLevelEmployeeAsync(int companyId)
        {
            var employees = await _employeeRepo
                .GetAll(e => e.CompanyId == companyId && e.IsActive)
                .Select(e => new
                {
                    EmployeeId = e.Id,
                    MaxLevel = e.EmployeeRoles
                        .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
                        .Select(er => (int?)er.Role.Level)
                        .Max() ?? (int)RoleLevelEnum.Employee
                })
                .ToListAsync();

            if (employees.Count == 0)
                return new List<int>();

            var topLevel = employees.Max(e => e.MaxLevel);

            return employees
                .Where(e => e.MaxLevel == topLevel)
                .OrderBy(e => e.EmployeeId)
                .Select(e => e.EmployeeId)
                .ToList();
        }
    }
}
