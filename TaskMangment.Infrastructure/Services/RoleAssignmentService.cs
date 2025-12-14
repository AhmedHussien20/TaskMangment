using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.ApiRequests.Role;
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
        public RoleAssignmentService(IRepository<Employee> employeeRepo,
            IRepository<Role> roleRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            ICachingService cache)
        {
            _employeeRepo = employeeRepo;
            _roleRepo = roleRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _cache = cache;
        }
        public async Task<ApiResponse<bool>> AssignEmployeesToRoleAsync(int roleId, RoleWithManyEmployeeAssignDto dto)
        {
            var role = await _roleRepo.GetByIDAsync(roleId);
            if (role == null)
                return ApiResponse<bool>.Fail("Role not found", StatusCode.NotFound);

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
                return ApiResponse<bool>.Fail(
                    $"Employees not found: {string.Join(", ", nonExistingEmployees)}",
                    StatusCode.NotFound
                );

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
                        return ApiResponse<bool>.Fail(
                            $"Employee {assignment.EmployeeId} is already assigned to another role",
                            StatusCode.BadRequest
                        );
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

            var roleExists = await _roleRepo
                .IsExistAsync(roleId);

            if (!roleExists)
                return ApiResponse<PagedResponse<AssignedEmployeeDto>>
                    .Fail("Role not found", StatusCode.NotFound);

            string cacheKey =
                $"assigned-employees:{roleId}:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<AssignedEmployeeDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<AssignedEmployeeDto>>.Ok(cached);
            }

            var query = _employeeRepo.GetAll()
                .Include(e => e.Branch)
                .Include(e => e.EmployeeRoles)
                .ApplySearch(request.searchKey);

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
                    er.RoleId == roleId && er.IsAssigned && !er.IsDeleted)
            }).ToList();

            var response = new PagedResponse<AssignedEmployeeDto>(
                result,
                totalCount,
                request.PageIndex,
                request.PageSize
            );

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<AssignedEmployeeDto>>.Ok(response);
        }


    }
}
