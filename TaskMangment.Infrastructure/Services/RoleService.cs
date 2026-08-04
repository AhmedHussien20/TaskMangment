using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Http;
using AutoMapper;

namespace TaskMangment.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IRepository<RoleNotificationSource> _notifySourceRepo;
        private readonly IRepository<EmployeeType> _employeeTypeRepo;
        private readonly IOrgManagerResolver _orgManagers;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public RoleService(
            IRepository<Role> roleRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<RolePermission> rolePermissionRepo,
            IRepository<RoleNotificationSource> notifySourceRepo,
            IRepository<EmployeeType> employeeTypeRepo,
            IOrgManagerResolver orgManagers)
        {
            _roleRepo = roleRepo;
            _mapper = mapper;
            _cache = cache;
            _employeeRoleRepo = employeeRoleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _notifySourceRepo = notifySourceRepo;
            _employeeTypeRepo = employeeTypeRepo;
            _orgManagers = orgManagers;
        }

        public async Task<ApiResponse<PagedResponse<RoleGetDto>>> GetAllAsync(RoleRequest request, int companyId)
        {
            var query = _roleRepo
                .GetAll(r => r.CompanyId == companyId)
                .Include(r => r.EmployeeType)
                .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var roles = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var roleIds = roles.Select(r => r.Id).ToList();

            var employeeCounts = await _employeeRoleRepo
                .GetAll(er => roleIds.Contains(er.RoleId) && er.IsAssigned)
                .GroupBy(er => er.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var permissionCounts = await _rolePermissionRepo
                .GetAll(rp => roleIds.Contains(rp.RoleId) && rp.IsAssigned)
                .GroupBy(rp => rp.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var notifySources = await _notifySourceRepo
                .GetAll(n => roleIds.Contains(n.RoleId) && !n.IsDeleted)
                .Select(n => new { n.RoleId, n.SourceRoleId })
                .ToListAsync();

            var rolesWithNotify = notifySources
                .Select(x => x.RoleId)
                .Distinct()
                .ToHashSet();

            var notifyFromCounts = await ComputeScopedNotifyFromCountsAsync(rolesWithNotify);

            var dtos = _mapper.Map<List<RoleGetDto>>(roles);

            foreach (var dto in dtos)
            {
                dto.EmployeeCount = employeeCounts.FirstOrDefault(x => x.RoleId == dto.Id)?.Count ?? 0;
                dto.PermissionCount = permissionCounts.FirstOrDefault(x => x.RoleId == dto.Id)?.Count ?? 0;
                dto.NotifyFromRoleIds = notifySources
                    .Where(x => x.RoleId == dto.Id)
                    .Select(x => x.SourceRoleId)
                    .Distinct()
                    .ToList();
                dto.NotifyFromEmployeeCount = notifyFromCounts.GetValueOrDefault(dto.Id);
            }

            var response = new PagedResponse<RoleGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);
            return ApiResponse<PagedResponse<RoleGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<RoleGetDto>> GetByIdAsync(int id, int companyId)
        {
            var role = await _roleRepo.GetAll(r => r.Id == id && r.CompanyId == companyId)
                .Include(r => r.EmployeeType)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            var dto = _mapper.Map<RoleGetDto>(role);
            dto.NotifyFromRoleIds = await _notifySourceRepo
                .GetAll(n => n.RoleId == id && !n.IsDeleted)
                .Select(n => n.SourceRoleId)
                .Distinct()
                .ToListAsync();

            if (dto.NotifyFromRoleIds.Count > 0)
            {
                var counts = await ComputeScopedNotifyFromCountsAsync(new HashSet<int> { id });
                dto.NotifyFromEmployeeCount = counts.GetValueOrDefault(id);
            }

            return ApiResponse<RoleGetDto>.Ok(dto);
        }

        /// <summary>
        /// Distinct subjects this role actually receives from (Branch / Area / Company rules),
        /// unioned across assignees — not the raw headcount of source roles.
        /// </summary>
        private async Task<Dictionary<int, int>> ComputeScopedNotifyFromCountsAsync(HashSet<int> listenerRoleIds)
        {
            var result = listenerRoleIds.ToDictionary(id => id, _ => 0);
            if (listenerRoleIds.Count == 0)
                return result;

            var assignees = await _employeeRoleRepo
                .GetAll(er =>
                    listenerRoleIds.Contains(er.RoleId) &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Employee != null &&
                    er.Employee.IsActive &&
                    !er.Employee.IsDeleted)
                .Select(er => new { er.RoleId, er.EmployeeId })
                .ToListAsync();

            foreach (var roleId in listenerRoleIds)
            {
                var assigneeIds = assignees
                    .Where(x => x.RoleId == roleId)
                    .Select(x => x.EmployeeId)
                    .Distinct()
                    .ToList();

                if (assigneeIds.Count == 0)
                    continue;

                var subjects = new HashSet<int>();
                foreach (var assigneeId in assigneeIds)
                {
                    var ids = await _orgManagers.GetListenableSubjectIdsAsync(assigneeId, roleId);
                    foreach (var subjectId in ids)
                        subjects.Add(subjectId);
                }

                result[roleId] = subjects.Count;
            }

            return result;
        }

        public async Task<ApiResponse<int>> CreateAsync(RoleAddEditDto dto, int companyId)
        {
            var roleExists = await _roleRepo
                .GetAll(r => r.CompanyId == companyId && r.Name == dto.Name)
                .AnyAsync();

            if (roleExists)
                throw new AppException(ErrorCodes.AlreadyExists, StatusCodes.Status400BadRequest);

            await EnsureRoleEmployeeTypeAsync(dto);

            var role = _mapper.Map<Role>(dto);
            role.CompanyId = companyId;
            role.Level = (int)RoleLevelEnum.Employee;
            role.NotificationScope = NotificationScope.None;

            await _roleRepo.AddAsync(role);
            await _roleRepo.SaveChangesAsync();

            await _cache.RemoveAsync("roles:");
            return ApiResponse<int>.Ok(role.Id, "Role created successfully");
        }

        public async Task<ApiResponse<RoleGetDto>> UpdateAsync(int id, RoleAddEditDto dto, int companyId)
        {
            var role = await _roleRepo.GetAll(r => r.Id == id && r.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            var roleExists = await _roleRepo
                .GetAll(r => r.CompanyId == companyId && r.Name == dto.Name && r.Id != id)
                .AnyAsync();

            if (roleExists)
                throw new AppException(ErrorCodes.AlreadyExists, StatusCodes.Status400BadRequest);

            await EnsureRoleEmployeeTypeAsync(dto);

            dto.CompanyId = companyId;
            _mapper.Map(dto, role);

            await _roleRepo.SaveChangesAsync();
            await _cache.RemoveAsync("roles:");

            return await GetByIdAsync(id, companyId);
        }

        private async Task EnsureRoleEmployeeTypeAsync(RoleAddEditDto dto)
        {
            if (!dto.RequiresEmployeeTypeScope)
            {
                dto.EmployeeTypeId = null;
                dto.RestrictEmployeeTypeToBranch = false;
                return;
            }

            if (!dto.EmployeeTypeId.HasValue || dto.EmployeeTypeId.Value <= 0)
                throw new AppException(ErrorCodes.RoleEmployeeTypeRequired, StatusCodes.Status400BadRequest);

            var exists = await _employeeTypeRepo
                .GetAll(t => t.Id == dto.EmployeeTypeId.Value && !t.IsDeleted)
                .AnyAsync();
            if (!exists)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status400BadRequest);
        }

        public async Task<ApiResponse<RoleGetDto>> UpdateNotificationsAsync(int id, RoleNotificationUpdateDto dto, int companyId)
        {
            var role = await _roleRepo.GetAll(r => r.Id == id && r.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            ValidateNotificationConfig(dto);

            role.NotificationScope = dto.NotificationScope;
            await ReplaceNotificationSourcesAsync(id, companyId, dto.NotifyFromRoleIds ?? new List<int>());
            await _roleRepo.SaveChangesAsync();
            await _cache.RemoveAsync("roles:");

            return await GetByIdAsync(id, companyId);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, int companyId)
        {
            var role = await _roleRepo.GetAll(r => r.Id == id && r.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            var hasEmployees = await _employeeRoleRepo.GetAll(er =>
                    er.RoleId == id && !er.IsDeleted && er.IsAssigned)
                .AnyAsync();

            if (hasEmployees)
                throw new AppException(ErrorCodes.RoleHasEmployees, StatusCodes.Status400BadRequest);

            var sources = await _notifySourceRepo
                .GetAll(n => (n.RoleId == id || n.SourceRoleId == id) && !n.IsDeleted)
                .ToListAsync();
            foreach (var s in sources)
                _notifySourceRepo.SoftDelete(s);

            _roleRepo.SoftDelete(role);
            await _roleRepo.SaveChangesAsync();
            await _cache.RemoveAsync("roles:");

            return ApiResponse<bool>.Ok(true, "Role deleted successfully");
        }

        private static void ValidateNotificationConfig(RoleNotificationUpdateDto dto)
        {
            var ids = dto.NotifyFromRoleIds ?? new List<int>();
            if (ids.Count > 0 && dto.NotificationScope == NotificationScope.None)
                throw new AppException(ErrorCodes.InvalidNotificationScope, StatusCodes.Status400BadRequest);

            if (ids.Count == 0)
                dto.NotificationScope = NotificationScope.None;
        }

        private async Task ReplaceNotificationSourcesAsync(int roleId, int companyId, List<int> sourceRoleIds)
        {
            var distinct = sourceRoleIds.Where(x => x > 0 && x != roleId).Distinct().ToList();

            if (distinct.Count > 0)
            {
                var validCount = await _roleRepo
                    .GetAll(r => r.CompanyId == companyId && distinct.Contains(r.Id) && !r.IsDeleted)
                    .CountAsync();
                if (validCount != distinct.Count)
                    throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);
            }

            var existing = await _notifySourceRepo
                .GetAll(n => n.RoleId == roleId)
                .ToListAsync();

            foreach (var row in existing)
            {
                if (distinct.Contains(row.SourceRoleId))
                {
                    row.IsDeleted = false;
                    row.DeletedDate = null;
                    distinct.Remove(row.SourceRoleId);
                }
                else if (!row.IsDeleted)
                {
                    _notifySourceRepo.SoftDelete(row);
                }
            }

            foreach (var sourceId in distinct)
            {
                await _notifySourceRepo.AddAsync(new RoleNotificationSource
                {
                    RoleId = roleId,
                    SourceRoleId = sourceId
                });
            }
        }
    }
}
