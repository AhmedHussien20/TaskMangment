using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class UserAccessContextProvider : IUserAccessContextProvider
    {
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<EmployeeFunctionalScope> _employeeFunctionalScopeRepo;

        public UserAccessContextProvider(IRepository<ManagerBranches> managerBranchesRepo, IRepository<EmployeeFunctionalScope> employeeFunctionalScopeRepo)
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

            var functionCodes = await _employeeFunctionalScopeRepo.GetAll()
                .Where(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .Select(x => x.FunctionCode)
                .ToListAsync();

            return new UserAccessContext
            {
                EmployeeId = employeeId,
                BranchIds = branchIds,
                FunctionCodes = functionCodes
            };
        }
    }

}
