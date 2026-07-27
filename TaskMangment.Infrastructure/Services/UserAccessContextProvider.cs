using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class UserAccessContextProvider : IUserAccessContextProvider
    {
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<EmployeeFunctionalScope> _employeeFunctionalScopeRepo;

        public UserAccessContextProvider(
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<EmployeeFunctionalScope> employeeFunctionalScopeRepo)
        {
            _managerBranchesRepo = managerBranchesRepo;
            _employeeFunctionalScopeRepo = employeeFunctionalScopeRepo;
        }

        public async Task<UserAccessContext> GetAsync(int employeeId)
        {
            var branchIds = await _managerBranchesRepo.GetAll()
                .Where(x => x.ManagerId == employeeId && x.IsActive)
                .Select(x => x.BranchId)
                .ToListAsync();

            var typeRows = await _employeeFunctionalScopeRepo.GetAll()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .Select(x => new
                {
                    x.EmployeeTypeId,
                    SeesAll = x.EmployeeType != null && x.EmployeeType.SeesAllTypesInBranchScope
                })
                .ToListAsync();

            var typeIds = typeRows.Select(x => x.EmployeeTypeId).Distinct().ToList();
            var seesAll = typeRows.Any(x => x.SeesAll);

            return new UserAccessContext
            {
                EmployeeId = employeeId,
                BranchIds = branchIds,
                EmployeeTypeIds = typeIds,
                SeesAllTypesInBranchScope = seesAll
            };
        }
    }
}
