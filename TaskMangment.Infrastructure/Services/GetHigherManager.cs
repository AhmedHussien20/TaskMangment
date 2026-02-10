using Microsoft.EntityFrameworkCore;
using System;
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

        public GetHigherManager(
            IRepository<Employee> employeeRepo,
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<EmployeeFunctionalScope> employeeFunctionScopeRepo)
        {
            _employeeRepo = employeeRepo;
            _managerBranchesRepo = managerBranchesRepo;
            _employeeFunctionScopeRepo = employeeFunctionScopeRepo;
        }

        public async Task<int?> GetDirectHigherManagerIdAsync(int currentEmployeeId)
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
                return currentEmployeeId;
            }

            int? higherManagerId = null;

            if (currentUser.FunctionCode == FunctionCode.Operations)
            {
                higherManagerId = await FindHigherManagerInBranchAsync(currentUser.Id, currentUser.CompanyId, currentUser.BranchId, currentUser.Level);
            }
            else 
            {
                higherManagerId = await FindHigherManagerByFunctionAsync(currentUser.Id, currentUser.CompanyId, currentUser.FunctionCode, currentUser.Level);
            }

            if (higherManagerId.HasValue)
            {
                return higherManagerId.Value;
            }

            var topEmployeeInCompany = await FindTopLevelEmployeeAsync(currentUser.CompanyId, currentUser.Id);

            return topEmployeeInCompany ?? currentEmployeeId;
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
                .Select(e => new {
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
                .Select(e => new {
                    EmployeeId = e.Id,
                    MaxLevel = e.EmployeeRoles.Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null).Select(er => (int?)er.Role.Level).Max() ?? (int)RoleLevelEnum.Employee
                })
                .Where(e => e.MaxLevel > currentLevel)
                .OrderBy(e => e.MaxLevel)
                .Select(e => (int?)e.EmployeeId)
                .FirstOrDefaultAsync();
        }

        private async Task<int?> FindTopLevelEmployeeAsync(int companyId, int currentEmployeeId)
        {
            return await _employeeRepo
                .GetAll(e => e.CompanyId == companyId && e.IsActive)
                .Select(e => new {
                    EmployeeId = e.Id,
                    MaxLevel = e.EmployeeRoles
                        .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
                        .Select(er => (int?)er.Role.Level)
                        .Max() ?? (int)RoleLevelEnum.Employee,
                    IsCurrent = e.Id == currentEmployeeId

                })
                 .OrderByDescending(e => e.MaxLevel)
                 .ThenByDescending(e => e.IsCurrent)
                .ThenBy(e => e.EmployeeId)
                .Select(e => (int?)e.EmployeeId)
                .FirstOrDefaultAsync();
        }
    }
}
