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
        private const int MaxRolesPerEmployee = 2;

        private readonly ICachingService _cache;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<EmployeeFunctionalScope> _employeeFunctionScopeRepo;
        private readonly IRepository<EmployeeType> _employeeTypeRepo;



        public RoleAssignmentService(IRepository<Employee> employeeRepo,
            IRepository<Role> roleRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            ICachingService cache,
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<Branch> branchRepo,
            IRepository<EmployeeFunctionalScope> employeeFunctionScopeRepo,
            IRepository<EmployeeType> employeeTypeRepo)
        {
            _employeeRepo = employeeRepo;
            _roleRepo = roleRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _cache = cache;
            _managerBranchesRepo = managerBranchesRepo;
            _branchRepo = branchRepo;
            _employeeFunctionScopeRepo = employeeFunctionScopeRepo;
            _employeeTypeRepo = employeeTypeRepo;
        }



        public async Task<ApiResponse<bool>> AssignEmployeesToRoleAsync(int roleId, RoleWithManyEmployeeAssignDto dto)
        {
            var role = await _roleRepo.GetAll(r => r.Id == roleId && !r.IsDeleted)
                .Select(r => new
                {
                    r.Id,
                    r.RequiresEmployeeTypeScope,
                    r.EmployeeTypeId
                })
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            if (role.RequiresEmployeeTypeScope &&
                (!role.EmployeeTypeId.HasValue || role.EmployeeTypeId.Value <= 0))
                throw new AppException(ErrorCodes.RoleEmployeeTypeRequired, StatusCodes.Status400BadRequest);

            var employeeIds = dto.Assignments
                .Select(a => a.EmployeeId)
                .Distinct()
                .ToList();

            var existingEmployees = await _employeeRepo
                .GetAll(e => employeeIds.Contains(e.Id) && !e.IsDeleted)
                .Select(e => new { e.Id, e.IsActive, e.EmployeeTypeId })
                .ToListAsync();

            var nonExistingEmployees = employeeIds.Except(existingEmployees.Select(e => e.Id)).ToList();
            if (nonExistingEmployees.Any())
                throw new AppException(
                    ErrorCodes.EmployeeNotFound,
                    StatusCodes.Status400BadRequest);

            var inactiveIds = existingEmployees.Where(e => !e.IsActive).Select(e => e.Id).ToList();
            if (inactiveIds.Any())
                throw new AppException(ErrorCodes.EmployeeInactive, StatusCodes.Status400BadRequest);

            foreach (var assignment in dto.Assignments)
            {
                if (assignment.Assign)
                {
                    if (role.RequiresEmployeeTypeScope)
                    {
                        var emp = existingEmployees.First(e => e.Id == assignment.EmployeeId);
                        if (emp.EmployeeTypeId != role.EmployeeTypeId)
                            throw new AppException(ErrorCodes.EmployeeTypeMismatch, StatusCodes.Status400BadRequest);
                    }

                    var employeeRoles = await _employeeRoleRepo
                        .GetAll(er =>
                            er.EmployeeId == assignment.EmployeeId &&
                            !er.IsDeleted)
                        .ToListAsync();

                    if (assignment.UnassignRoleIds?.Count > 0)
                    {
                        foreach (var otherRoleId in assignment.UnassignRoleIds.Distinct())
                        {
                            if (otherRoleId == roleId)
                                continue;

                            var other = employeeRoles.FirstOrDefault(er => er.RoleId == otherRoleId);
                            if (other != null)
                                other.IsAssigned = false;
                        }
                    }

                    var alreadyAssignedToThisRole = employeeRoles.Any(er =>
                        er.RoleId == roleId && er.IsAssigned);

                    if (alreadyAssignedToThisRole)
                        continue;

                    var assignedRoleCount = employeeRoles.Count(er => er.IsAssigned);
                    if (assignedRoleCount >= MaxRolesPerEmployee)
                        throw new AppException(
                            ErrorCodes.EmployeeMaxRolesExceeded,
                            StatusCodes.Status400BadRequest);

                    var employeeRole = employeeRoles.FirstOrDefault(er => er.RoleId == roleId);
                    if (employeeRole != null)
                    {
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
                        .GetAll(er =>
                            er.EmployeeId == assignment.EmployeeId &&
                            er.RoleId == roleId &&
                            !er.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (employeeRole != null)
                    {
                        employeeRole.IsAssigned = false;
                    }
                }
            }

            await _employeeRoleRepo.SaveChangesAsync();

            foreach (var employeeId in employeeIds)
                await ReconcileEmployeeTypeScopeAsync(employeeId);

            await _employeeFunctionScopeRepo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Employees assigned/unassigned successfully");
        }

        private async Task ReconcileEmployeeTypeScopeAsync(int employeeId)
        {
            var assignedRoleIds = await _employeeRoleRepo
                .GetAll(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted)
                .Select(er => er.RoleId)
                .ToListAsync();

            var typeIds = assignedRoleIds.Count == 0
                ? new List<int>()
                : await _roleRepo
                    .GetAll(r =>
                        assignedRoleIds.Contains(r.Id) &&
                        !r.IsDeleted &&
                        r.RequiresEmployeeTypeScope &&
                        r.EmployeeTypeId != null)
                    .Select(r => r.EmployeeTypeId!.Value)
                    .Distinct()
                    .ToListAsync();

            var existingScopes = await _employeeFunctionScopeRepo
                .GetAll(x => x.EmployeeId == employeeId && !x.IsDeleted)
                .ToListAsync();

            if (typeIds.Count == 0)
            {
                foreach (var scope in existingScopes)
                    _employeeFunctionScopeRepo.SoftDelete(scope);
                return;
            }

            // One active functional scope per employee (matches existing assign/upsert behavior).
            var typeId = typeIds[0];
            var keep = existingScopes.FirstOrDefault();
            if (keep != null)
            {
                keep.EmployeeTypeId = typeId;
                keep.IsDeleted = false;
                keep.DeletedDate = null;
                foreach (var extra in existingScopes.Skip(1))
                    _employeeFunctionScopeRepo.SoftDelete(extra);
            }
            else
            {
                await _employeeFunctionScopeRepo.AddAsync(new EmployeeFunctionalScope
                {
                    EmployeeId = employeeId,
                    EmployeeTypeId = typeId
                });
            }
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

        

            var query = _employeeRepo.GetAll(e => e.IsActive && !e.IsDeleted)
    .Include(e => e.Branch)
.Include(e => e.EmployeeRoles)
    .ThenInclude(er => er.Role)
    .ApplySearch(request.searchKey);

            var roleType = await _roleRepo.GetAll(r => r.Id == roleId && !r.IsDeleted)
                .Select(r => new { r.RequiresEmployeeTypeScope, r.EmployeeTypeId })
                .FirstOrDefaultAsync();

            // For type-scoped roles, only list employees of that type (plus already assigned).
            if (roleType?.RequiresEmployeeTypeScope == true && roleType.EmployeeTypeId.HasValue)
            {
                var typeId = roleType.EmployeeTypeId.Value;
                query = query.Where(e =>
                    e.EmployeeTypeId == typeId ||
                    e.EmployeeRoles.Any(er =>
                        er.RoleId == roleId && er.IsAssigned && !er.IsDeleted));
            }

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

            var result = employees.Select(e =>
            {
                var assignedRoles = e.EmployeeRoles
                    .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
                    .GroupBy(er => er.RoleId)
                    .Select(g => new AssignedRoleItemDto
                    {
                        RoleId = g.Key,
                        RoleName = g.First().Role!.Name
                    })
                    .ToList();

                return new AssignedEmployeeDto
                {
                    EmployeeId = e.Id,
                    FullName = e.FullName,
                    Email = e.Email ?? "",
                    Mobile = e.Mobile ?? "",
                    BranchName = e.Branch?.Name,
                    IsAssigned = assignedRoles.Any(r => r.RoleId == roleId),
                    RoleName = string.Join(", ", assignedRoles.Select(r => r.RoleName)),
                    AssignedRoles = assignedRoles
                };
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

            var managerActive = await _employeeRepo.GetAll(e => e.Id == managerId && !e.IsDeleted)
                .Select(e => e.IsActive)
                .FirstOrDefaultAsync();
            if (!managerActive)
                throw new AppException(ErrorCodes.EmployeeInactive, StatusCodes.Status400BadRequest);

            var roleScope = await _employeeRepo.GetAll(e => e.Id == managerId)
                .Select(e => new
                {
                    RequiresBranchScope = e.EmployeeRoles
                        .Where(er => !er.IsDeleted && er.IsAssigned && er.Role != null)
                        .Any(er => er.Role!.RequiresBranchScope),
                    RequiresEmployeeTypeScope = e.EmployeeRoles
                        .Where(er => !er.IsDeleted && er.IsAssigned && er.Role != null)
                        .Any(er => er.Role!.RequiresEmployeeTypeScope)
                })
                .FirstAsync();

            if (!roleScope.RequiresBranchScope && !roleScope.RequiresEmployeeTypeScope)
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            // Branch coverage for RequiresBranchScope is managed via Area page, not نطاق التغطية.
            if (roleScope.RequiresBranchScope && !roleScope.RequiresEmployeeTypeScope)
                throw new AppException(ErrorCodes.UseAreaForBranchScope, StatusCodes.Status400BadRequest);

            // Type coverage is fixed by the role's EmployeeTypeId — not chosen per assignee.
            var roleTypeId = await _employeeRoleRepo
                .GetAll(er =>
                    er.EmployeeId == managerId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted &&
                    er.Role.RequiresEmployeeTypeScope &&
                    er.Role.EmployeeTypeId != null)
                .Select(er => er.Role!.EmployeeTypeId!.Value)
                .FirstOrDefaultAsync();

            if (roleTypeId <= 0)
                throw new AppException(ErrorCodes.RoleEmployeeTypeRequired, StatusCodes.Status400BadRequest);

            var managerTypeId = await _employeeRepo.GetAll(e => e.Id == managerId)
                .Select(e => e.EmployeeTypeId)
                .FirstOrDefaultAsync();
            if (managerTypeId != roleTypeId)
                throw new AppException(ErrorCodes.EmployeeTypeMismatch, StatusCodes.Status400BadRequest);

            // Ignore client-picked type; always use the role's type.
            int employeeTypeId = roleTypeId;
            var employeeType = await _employeeTypeRepo
                .GetAll(t => t.Id == employeeTypeId && !t.IsDeleted)
                .Select(t => new
                {
                    t.Id,
                    SeesAll = t.SeesAllTypesInBranchScope || t.Code == EmployeeTypeCodes.Operations
                })
                .FirstOrDefaultAsync();

            if (employeeType == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status400BadRequest);

            bool seesAllTypesInBranchScope = employeeType.SeesAll;
            _ = seesAllTypesInBranchScope;

            var existingScope = await _employeeFunctionScopeRepo
                .GetAll(x => x.EmployeeId == managerId && !x.IsDeleted)
                .FirstOrDefaultAsync();

            if (existingScope != null)
            {
                existingScope.EmployeeTypeId = employeeTypeId;
                existingScope.IsDeleted = false;
                existingScope.DeletedDate = null;
            }
            else
            {
                await _employeeFunctionScopeRepo.AddAsync(new EmployeeFunctionalScope
                {
                    EmployeeId = managerId,
                    EmployeeTypeId = employeeTypeId,
                });
            }

            // Type coverage only — clear any legacy ManagerBranches rows.
            var activeBranches = await _managerBranchesRepo
                .GetAll(x => x.ManagerId == managerId && x.IsActive)
                .ToListAsync();
            foreach (var item in activeBranches)
                item.IsActive = false;

            await _employeeFunctionScopeRepo.SaveChangesAsync();
            await _managerBranchesRepo.SaveChangesAsync();

            return ApiResponse<ManagerBranchesDto>.Ok(new ManagerBranchesDto
            {
                ManagerId = managerId,
                BranchIds = new List<int>()
            });
        }

        public async Task<ApiResponse<GetManagerBranchesDto>> GetManagerBranchesAsync(int managerId)
        {
            if (!await _employeeRepo.IsExistAsync(managerId))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status400BadRequest);

            var companyId = await _employeeRepo.GetAll(e => e.Id == managerId)
                .Select(e => e.CompanyId)
                .FirstAsync();

            var currentBranchIds = await _managerBranchesRepo
                .GetAll(x =>
                    x.ManagerId == managerId &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    x.Branch != null &&
                    !x.Branch.IsDeleted &&
                    x.Branch.IsActive)
                .Select(x => x.BranchId)
                .ToListAsync();

            var employeeTypeId = await _employeeFunctionScopeRepo
                .GetAll(x => x.EmployeeId == managerId && !x.IsDeleted)
                .Select(x => (int?)x.EmployeeTypeId)
                .FirstOrDefaultAsync();

            // Available = not actively covered by another RequiresBranchScope manager.
            // Type-scoped / branch-owner ManagerBranches rows do not block (e.g. سكاكا held by Branch Manager 83).
            var takenByOthers = await _managerBranchesRepo
                .GetAll(x =>
                    x.IsActive &&
                    !x.IsDeleted &&
                    x.ManagerId != managerId &&
                    x.Branch != null &&
                    !x.Branch.IsDeleted &&
                    x.Branch.IsActive &&
                    x.Manager != null &&
                    x.Manager.EmployeeRoles.Any(er =>
                        !er.IsDeleted &&
                        er.IsAssigned &&
                        er.Role != null &&
                        er.Role.RequiresBranchScope))
                .Select(x => x.BranchId)
                .Distinct()
                .ToListAsync();

            var takenSet = takenByOthers.ToHashSet();

            var availableBranches = await _branchRepo
                .GetAll(b =>
                    !b.IsDeleted &&
                    b.IsActive &&
                    b.CompanyId == companyId &&
                    !takenSet.Contains(b.Id))
                .Select(b => new BranchLookupDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .ToListAsync();

            var currentBranches = await _branchRepo
                .GetAll(b => currentBranchIds.Contains(b.Id) && !b.IsDeleted && b.IsActive)
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
                .OrderBy(x => x.Name)
                .ToList();

            var roleFlags = await _employeeRepo.GetAll(e => e.Id == managerId)
                .Select(e => new
                {
                    RequiresBranchScope = e.EmployeeRoles
                        .Where(er => !er.IsDeleted && er.IsAssigned && er.Role != null)
                        .Any(er => er.Role!.RequiresBranchScope),
                    RequiresEmployeeTypeScope = e.EmployeeRoles
                        .Where(er => !er.IsDeleted && er.IsAssigned && er.Role != null)
                        .Any(er => er.Role!.RequiresEmployeeTypeScope)
                })
                .FirstAsync();

            return ApiResponse<GetManagerBranchesDto>.Ok(new GetManagerBranchesDto
            {
                ManagerId = managerId,
                EmployeeTypeId = employeeTypeId,
                BranchIds = currentBranchIds,
                branchLookupDtos = combined,
                RequiresBranchScope = roleFlags.RequiresBranchScope,
                RequiresEmployeeTypeScope = roleFlags.RequiresEmployeeTypeScope
            });
        }

    }
}
