using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class RoleAssignmentService : IRoleAssignmentService
    {
        private readonly ICachingService _cache;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<EmployeeFunctionalScope> _employeeFunctionScopeRepo;



        public RoleAssignmentService(IRepository<Employee> employeeRepo,
            IRepository<Role> roleRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            ICachingService cache,
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<Branch> branchRepo,
            IRepository<EmployeeFunctionalScope> employeeFunctionScopeRepo)
        {
            _employeeRepo = employeeRepo;
            _roleRepo = roleRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _cache = cache;
            _managerBranchesRepo = managerBranchesRepo;
            _branchRepo = branchRepo;
            _employeeFunctionScopeRepo = employeeFunctionScopeRepo;
        }



        public async Task<ApiResponse<bool>> AssignEmployeesToRoleAsync(int roleId, RoleWithManyEmployeeAssignDto dto)
        {
            if (!await _roleRepo.IsExistAsync(roleId))
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            var employeeIds = dto.Assignments
                .Select(a => a.EmployeeId)
                .Distinct()
                .ToList();

            var existingEmployees = await _employeeRepo
                .GetAll(e => employeeIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync();

            var nonExistingEmployees = employeeIds.Except(existingEmployees).ToList();
            if (nonExistingEmployees.Any())
                throw new AppException(
                    ErrorCodes.EmployeeNotFound,
                    StatusCodes.Status400BadRequest);

            foreach (var assignment in dto.Assignments)
            {
                if (assignment.Assign)
                {
                    var activeRole = await _employeeRoleRepo
                        .GetAll(er =>
                            er.EmployeeId == assignment.EmployeeId &&
                            er.IsAssigned)
                        .FirstOrDefaultAsync();

                    if (activeRole != null)
                    {
                        throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status400BadRequest);
                    }

                    var employeeRole = await _employeeRoleRepo
                        .GetAll(er => er.EmployeeId == assignment.EmployeeId)
                        .FirstOrDefaultAsync();

                    if (employeeRole != null)
                    {
                        employeeRole.RoleId = roleId;
                        employeeRole.IsAssigned = true;
                        employeeRole.IsDeleted = false;
                        employeeRole.DeletedDate = null;
                    }
                    else
                    {
                        await _employeeRoleRepo.AddAsync(new EmployeeRole
                        {
                            EmployeeId = assignment.EmployeeId,
                            RoleId = roleId,
                            CreatedDate = DateTime.UtcNow,
                            IsAssigned = true
                        });
                    }
                }
                else
                {
                    var employeeRole = await _employeeRoleRepo
                        .GetAll(er => er.EmployeeId == assignment.EmployeeId)
                        .FirstOrDefaultAsync();

                    if (employeeRole != null)
                    {
                        employeeRole.IsAssigned = false;
                    }
                }
            }

            await _employeeRoleRepo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Employees assigned/unassigned successfully");
        }
        public async Task<ApiResponse<PagedResponse<AssignedEmployeeDto>>>GetAssignedEmployeesPagedAsync(int roleId, RoleAssignmentReguest request)
        {

            if (!await _roleRepo.IsExistAsync(roleId))
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            //string cacheKey =
            //    $"assigned-employees:{roleId}:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{request.IsAssigned}";


            //if (!request.BypassCache)
            //{
            //    var cached = await _cache.GetAsync<PagedResponse<AssignedEmployeeDto>>(cacheKey);
            //    if (cached != null)
            //        return ApiResponse<PagedResponse<AssignedEmployeeDto>>.Ok(cached);
            //}

        

            var query = _employeeRepo.GetAll()
    .Include(e => e.Branch)
.Include(e => e.EmployeeRoles)
    .ThenInclude(er => er.Role)
    .ApplySearch(request.searchKey);

            if (request.IsAssigned.HasValue)
            {
                if (request.IsAssigned.Value)
                {
                    query = query.Where(e =>
                        e.EmployeeRoles.Any(er =>
                            er.RoleId == roleId &&
                            er.IsAssigned &&
                            !er.IsDeleted));
                }
                else
                {
                    query = query.Where(e =>
                        !e.EmployeeRoles.Any(er =>
                            er.RoleId == roleId &&
                            er.IsAssigned &&
                            !er.IsDeleted));
                }
            }


            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var employees = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var result = employees.Select(e => new AssignedEmployeeDto
            {
                EmployeeId = e.Id,
                FullName = e.FullName,
                Email = e.Email ?? "",
                Mobile = e.Mobile ?? "",
                BranchName = e.Branch?.Name,

                IsAssigned = e.EmployeeRoles.Any(er =>
                    er.RoleId == roleId && er.IsAssigned && !er.IsDeleted),

                RoleName = e.EmployeeRoles
        .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
        .Select(er => er.Role!.Name)
        .FirstOrDefault()
            }).ToList();

            var response = new PagedResponse<AssignedEmployeeDto>(
                result,
                totalCount,
                request.PageIndex,
                request.PageSize
            );

           // await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<AssignedEmployeeDto>>.Ok(response);
        }

        public async Task<List<UserRoleDto>> GetUserRolesAsync(int userId)
        {
            var roles = await _employeeRoleRepo
                .GetAll(er =>
                    er.EmployeeId == userId &&
                    !er.IsDeleted && er.IsAssigned)
                .Include(er => er.Role)
                .AsNoTracking()
                .Select(er => new UserRoleDto
                {
                    Name = er.Role.Name,
                    Level = er.Role.Level
                })
                .ToListAsync();

            return roles;
        }


        public async Task<int> GetUserMaxRoleLevelAsync(int userId)
        {
            return await _employeeRoleRepo
                .GetAll(er => er.EmployeeId == userId && er.IsAssigned)
                .Join(_roleRepo.GetAll(),
                      er => er.RoleId,
                      r => r.Id,
                      (er, r) => r.Level)
                .DefaultIfEmpty(0)
                .MaxAsync();
        }
        public async Task<ApiResponse<ManagerBranchesDto>> SetManagerBranchesAsync(
     int managerId,
     SetManagerBranchesRequest request)
        {
            if (!await _employeeRepo.IsExistAsync(managerId))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status400BadRequest);

            var functionCode = (FunctionCode)request.FunctionCode;

            var existingScope = await _employeeFunctionScopeRepo
                .GetAll(x => x.EmployeeId == managerId && !x.IsDeleted)
                .FirstOrDefaultAsync();

            if (existingScope != null)
            {
                existingScope.FunctionCode = functionCode;
                existingScope.IsDeleted = false;
                existingScope.DeletedDate = null;
            }
            else
            {
                await _employeeFunctionScopeRepo.AddAsync(new EmployeeFunctionalScope
                {
                    EmployeeId = managerId,
                    FunctionCode = functionCode,
                });
            }

            if (functionCode != FunctionCode.Operations)
            {
                await _employeeFunctionScopeRepo.SaveChangesAsync();

                return ApiResponse<ManagerBranchesDto>.Ok(new ManagerBranchesDto
                {
                    ManagerId = managerId,
                    BranchIds = new List<int>()
                });
            }

            var hasRoleLevel80 = await _employeeRepo
                .GetAll(e => e.Id == managerId)
                .SelectMany(e => e.EmployeeRoles)
                .Where(er => !er.IsDeleted)
                .AnyAsync(er => er.Role != null && er.Role.Level == 80);

            if (!hasRoleLevel80)
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            var branchIds = (request.BranchIds ?? new List<int>())
                .Distinct()
                .ToList();

            if (branchIds.Any())
            {
                var conflicts = await _managerBranchesRepo
                    .GetAll(x => branchIds.Contains(x.BranchId)
                                 && x.IsActive
                                 && x.ManagerId != managerId)
                    .Select(x => x.BranchId)
                    .Distinct()
                    .ToListAsync();

                if (conflicts.Any())
                    throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status409Conflict);
            }

            var existingActive = await _managerBranchesRepo
                .GetAll(x => x.ManagerId == managerId && x.IsActive)
                .ToListAsync();

            var existingActiveIds = existingActive.Select(x => x.BranchId).ToHashSet();

            var toDeactivate = existingActive.Where(x => !branchIds.Contains(x.BranchId)).ToList();
            foreach (var item in toDeactivate)
            {
                item.IsActive = false;
            }

            var toAdd = branchIds.Where(id => !existingActiveIds.Contains(id)).ToList();

            if (toAdd.Any())
            {
               
                var existingAny = await _managerBranchesRepo
                    .GetAll(x => x.ManagerId == managerId && toAdd.Contains(x.BranchId))
                    .ToListAsync();

                var map = existingAny.ToDictionary(x => x.BranchId);

                foreach (var bId in toAdd)
                {
                    if (map.TryGetValue(bId, out var row))
                    {
                        row.IsActive = true;
                    }
                    else
                    {
                        await _managerBranchesRepo.AddAsync(new ManagerBranches
                        {
                            ManagerId = managerId,
                            BranchId = bId,
                            IsActive = true,
                        });
                    }
                }
            }

            await _employeeFunctionScopeRepo.SaveChangesAsync();
            await _managerBranchesRepo.SaveChangesAsync(); // ✅ لازم

            return ApiResponse<ManagerBranchesDto>.Ok(new ManagerBranchesDto
            {
                ManagerId = managerId,
                BranchIds = branchIds
            });
        }

        public async Task<ApiResponse<GetManagerBranchesDto>> GetManagerBranchesAsync(int managerId)
        {
            if (!await _employeeRepo.IsExistAsync(managerId))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status400BadRequest);

            var currentBranchIds = await _managerBranchesRepo
                .GetAll(x => x.ManagerId == managerId && x.IsActive)
                .Select(x => x.BranchId)
                .ToListAsync();

            var functionCode = await _employeeFunctionScopeRepo
                .GetAll(x => x.EmployeeId == managerId && !x.IsDeleted)
                .Select(x => x.FunctionCode)
                .FirstOrDefaultAsync();

            var assignedBranchIds = await _managerBranchesRepo
                .GetAll(x => x.IsActive)
                .Select(x => x.BranchId)
                .Distinct()
                .ToListAsync();

            var availableBranches = await _branchRepo
                .GetAll(b => !assignedBranchIds.Contains(b.Id))
                .Select(b => new BranchLookupDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .ToListAsync();

            var currentBranches = await _branchRepo
                .GetAll(b => currentBranchIds.Contains(b.Id))
                .Select(b => new BranchLookupDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .ToListAsync();

            var combined = availableBranches
                .Concat(currentBranches)
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();

            return ApiResponse<GetManagerBranchesDto>.Ok(new GetManagerBranchesDto
            {
                ManagerId = managerId,
                FunctionCode = functionCode,
                BranchIds = currentBranchIds,
                branchLookupDtos = combined
            });
        }


    }
}
