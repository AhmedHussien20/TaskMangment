using AutoMapper;
using Microsoft.AspNetCore.Http; 
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Common.ApiRequests.Leave;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Entities.Enum;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class LeaveService : ILeaveService
    {
        private const string LeaveCalendarDescriptionPrefix = "LeaveId:";

        private readonly IRepository<Leave> _leaveRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<LeaveType> _leaveTypeRepo;
        private readonly IRepository<CalendarEvent> _calendarEventRepo;
        private readonly IMapper _mapper;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IRepository<Employee> _empRepo;
        private readonly IUserAccessContextProvider _accessProvider;
        private readonly IGetHigherManager _getHigherManager;
        private readonly IAccessScopeResolver _scopeResolver;
        private readonly IEmployeePermissionService _permissions;
        private readonly IOrgManagerResolver _orgManagers;


        public LeaveService(
            IRepository<Leave> leaveRepo,
            IRepository<Employee> employeeRepo,
            IRepository<LeaveType> leaveTypeRepo,
            IRepository<CalendarEvent> calendarEventRepo,
            IMapper mapper,
            IDomainEventDispatcher eventDispatcher,
            IRepository<Employee> empRepo,
            IUserAccessContextProvider accessProvider,
            IGetHigherManager getHigherManager,
            IAccessScopeResolver scopeResolver,
            IEmployeePermissionService permissions,
            IOrgManagerResolver orgManagers
            )
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _calendarEventRepo = calendarEventRepo;
            _mapper = mapper;
            _eventDispatcher = eventDispatcher;
            _empRepo = empRepo;
            _accessProvider = accessProvider;
            _getHigherManager = getHigherManager;
            _scopeResolver = scopeResolver;
            _permissions = permissions;
            _orgManagers = orgManagers;
        }
         
        public async Task<ApiResponse<LeaveGetDto>> CreateAsync(LeaveAddDto dto, int employeeId)
        {
            if (!await _employeeRepo.IsExistAsync(employeeId))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound);

            if (!await _leaveTypeRepo.IsExistAsync(dto.LeaveTypeId))
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (dto.EndDate < dto.StartDate)
                throw new AppException(ErrorCodes.InvalidDate, StatusCodes.Status400BadRequest);

            var leave = new Leave
            {
                EmployeeId = employeeId,
                LeaveTypeId = dto.LeaveTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Notes = dto.Notes,
                Status = LeaveStatus.Pending
            };
          

            await _leaveRepo.AddAsync(leave);
            await _leaveRepo.SaveChangesAsync();

            var full = await _leaveRepo.GetAll(l => l.Id == leave.Id)
                .Include(l => l.Employee)
                .ThenInclude(e => e.Branch)
                .Include(l => l.LeaveType)
                .FirstAsync();

            // Higher managers + RoleNotificationSource listeners (may be empty).
            var recipientIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(employeeId);
            recipientIds = recipientIds?.Where(id => id != employeeId).Distinct().ToList()
                ?? new List<int>();

            foreach (var recipientId in recipientIds)
            {
                await _eventDispatcher.PublishAsync(new LeaveEvent(
                                                      leave.Id,
                                                      recipientId,
                                                      full.Employee.FullName,
                                                      full.LeaveType.NameAr,
                                                      leave.StartDate,
                                                      leave.EndDate,
                                                      full.Employee.Branch?.Name
                                                    ));
            }

            var result = _mapper.Map<LeaveGetDto>(full);
            result.CanDelete = true;

            return ApiResponse<LeaveGetDto>.Ok(result, "Leave request submitted");
        }

        public async Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetLeaveRequestsAsync(
      LeaveRequest request,
      int roleLevel,
      int employeeId)
        {
            var query = _leaveRepo.GetAll()
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .ApplySearch(request.searchKey);

            if (request.StatusId.HasValue)
                query = query.Where(l => l.Status == (LeaveStatus)request.StatusId.Value);

            _ = roleLevel;
            var canReviewLeave = await _permissions.HasAnyAsync(
                employeeId,
                PermissionCodes.ApproveLeave,
                PermissionCodes.RejectLeave);

            var scope = await _scopeResolver.ResolveAsync(employeeId);
            var canViewScoped =
                scope.Kind is AccessScopeKind.ManagerScoped or AccessScopeKind.CompanyWide
                || await _permissions.HasAsync(employeeId, PermissionCodes.ViewScopedTasks)
                || await _permissions.HasAsync(employeeId, PermissionCodes.ViewCompanyTasks);

            // جميع الاجازات = AccessScope (like tasks). Default reviewer list stays notification-based.
            if (request.ViewScopedLeaves == true && canViewScoped)
            {
                var scopedEmployeeIds = _scopeResolver
                    .FilterEmployees(
                        _employeeRepo.GetAll(e => e.CompanyId == scope.CompanyId && e.IsActive),
                        scope)
                    .Select(e => e.Id);

                query = query.Where(l => scopedEmployeeIds.Contains(l.EmployeeId));

                if (request.EmployeeIds != null && request.EmployeeIds.Any())
                    query = query.Where(l => request.EmployeeIds.Contains(l.EmployeeId));
            }
            else if (!canReviewLeave)
            {
                query = query.Where(l => l.EmployeeId == employeeId);
            }
            else
            {
                var visibleEmployeeIds = await GetLeaveVisibleEmployeeIdsAsync(employeeId);
                query = query.Where(l => visibleEmployeeIds.Contains(l.EmployeeId));

                if (request.EmployeeIds != null && request.EmployeeIds.Any())
                    query = query.Where(l => request.EmployeeIds.Contains(l.EmployeeId));
            }

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(
                request.SortColumn ?? "CreatedDate",
                request.SortDirection ?? "DESC");

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<LeaveGetDto>>(list);
            await ApplyCanDeleteAsync(dtos, list, employeeId);

            var response = new PagedResponse<LeaveGetDto>(
                dtos,
                totalCount,
                request.PageIndex,
                request.PageSize);

            return ApiResponse<PagedResponse<LeaveGetDto>>.Ok(response);
        }


        public async Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetPendingForApprovalAsync(int managerId, LeaveRequest request)
        {
            var visibleEmployeeIds = await GetLeaveVisibleEmployeeIdsAsync(managerId);

            IQueryable<Leave> query = _leaveRepo.GetAll(l =>
                    l.Status == LeaveStatus.Pending &&
                    visibleEmployeeIds.Contains(l.EmployeeId))
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .ApplySearch(request.searchKey);

            if (request.EmployeeIds != null && request.EmployeeIds.Any())
                query = query.Where(l => request.EmployeeIds.Contains(l.EmployeeId));

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(
                request.SortColumn ?? "CreatedDate",
                request.SortDirection ?? "ASC");

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<LeaveGetDto>>(list);
            await ApplyCanDeleteAsync(dtos, list, managerId);

            var response = new PagedResponse<LeaveGetDto>(
                dtos, totalCount, request.PageIndex, request.PageSize);

            return ApiResponse<PagedResponse<LeaveGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int leaveId, int actorId)
        {
            var leave = await _leaveRepo.GetAll(l => l.Id == leaveId)
                .FirstOrDefaultAsync();

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (leave.Status != LeaveStatus.Pending)
                throw new AppException(ErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest);

            await EnsureCanDeleteLeaveAsync(actorId, leave);

            _leaveRepo.SoftDelete(leave);
            await _leaveRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Leave request deleted");
        }

        // =========================
        // Approve
        // =========================
        public async Task<ApiResponse<bool>> ApproveAsync(int leaveId, int managerId, string managerFullName)
        {
            var leave = await _leaveRepo.GetAll(l => l.Id == leaveId)
                .Include(l => l.Employee)
                .ThenInclude(e => e.Branch)
                .Include(l => l.ApprovedBy)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync();

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (leave.Status != LeaveStatus.Pending)
                throw new AppException(ErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest);

            var manager = await _empRepo.GetByIDAsync(managerId);
            if (manager == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);



            await EnsureCanApproveLeaveAsync(managerId, leave.EmployeeId);

            leave.Status = LeaveStatus.Approved;
            leave.ApprovedById = managerId;
            leave.ApprovedAt = DateTime.UtcNow;

            await _leaveRepo.SaveChangesAsync();

            await EnsureLeaveCalendarEventAsync(leave, managerId);

            await _eventDispatcher.PublishAsync(new LeaveApprovedEvent(
                leave.Id,
                leave.EmployeeId,
                managerId,
                leave.Employee.FullName,
                managerFullName,
                leave.LeaveType.NameAr,
                leave.StartDate,
                leave.EndDate,
                leave.Employee.Branch?.Name
            ));

            return ApiResponse<bool>.Ok(true, "Leave approved");
        }



        public async Task<ApiResponse<bool>> RejectAsync(int leaveId, int managerId, RejectLeaveDto rejectLeaveDto)
        {
            var leave = await _leaveRepo.GetAll(l => l.Id == leaveId)
                .Include(l => l.Employee)
                .ThenInclude(e => e.Branch)
                .Include(l => l.ApprovedBy)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync();

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (leave.Status != LeaveStatus.Pending)
                throw new AppException(ErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(rejectLeaveDto.reason))
                throw new AppException(ErrorCodes.LeaveRejectReasonRequired, StatusCodes.Status400BadRequest);

            var manager = await _empRepo.GetByIDAsync(managerId);
            if (manager == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            await EnsureCanRejectLeaveAsync(managerId, leave.EmployeeId);

            leave.Status = LeaveStatus.Rejected;
            leave.RejectionReason = rejectLeaveDto.reason;
            leave.ApprovedById = managerId;
            leave.ApprovedAt = DateTime.UtcNow;

            await _leaveRepo.SaveChangesAsync();

            await _eventDispatcher.PublishAsync(new LeaveRejectedEvent(
                leave.Id,
                leave.EmployeeId,
                managerId,
                leave.Employee.FullName,
                manager.FullName,
                leave.LeaveType.NameAr,
                leave.RejectionReason,
                leave.StartDate,
                leave.EndDate,
                leave.Employee.Branch?.Name
            ));

            return ApiResponse<bool>.Ok(true, "Leave rejected");
        }


        public async Task<ApiResponse<LeaveGetDto>> GetByIdAsync(int leaveId, int actorId)
        {
            var leave = await _leaveRepo.GetAll(l => l.Id == leaveId)
                                        .Include(l => l.Employee)
                                        .Include(l => l.LeaveType)
                                        .FirstOrDefaultAsync();

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var dto = _mapper.Map<LeaveGetDto>(leave);
            dto.CanDelete = await CanDeleteLeaveAsync(leave, actorId);

            return ApiResponse<LeaveGetDto>.Ok(dto);
        }

        private async Task EnsureCanDeleteLeaveAsync(int actorId, Leave leave)
        {
            if (await CanDeleteLeaveAsync(leave, actorId))
                return;

            throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);
        }

        private async Task ApplyCanDeleteAsync(
            ICollection<LeaveGetDto> dtos,
            ICollection<Leave> leaves,
            int actorId)
        {
            var scopedEmployeeIds = await GetScopedEmployeeIdsAsync(actorId);

            foreach (var dto in dtos)
            {
                var leave = leaves.First(l => l.Id == dto.Id);
                dto.CanDelete = await CanDeleteLeaveAsync(
                    leave,
                    actorId,
                    scopedEmployeeIds);
            }
        }

        private async Task<bool> CanDeleteLeaveAsync(
            Leave leave,
            int actorId,
            HashSet<int>? scopedEmployeeIds = null)
        {
            if (leave.Status != LeaveStatus.Pending)
                return false;

            if (leave.EmployeeId == actorId)
                return true;

            if (!await _permissions.HasAnyAsync(
                    actorId,
                    PermissionCodes.ApproveLeave,
                    PermissionCodes.RejectLeave))
                return false;

            var visibleIds = await GetLeaveVisibleEmployeeIdsAsync(actorId);
            if (visibleIds.Contains(leave.EmployeeId))
                return true;

            scopedEmployeeIds ??= await GetScopedEmployeeIdsAsync(actorId);
            if (scopedEmployeeIds != null && scopedEmployeeIds.Contains(leave.EmployeeId))
                return true;

            return false;
        }

        private async Task<HashSet<int>?> GetScopedEmployeeIdsAsync(int actorId)
        {
            var scope = await _scopeResolver.ResolveAsync(actorId);
            var canViewScoped =
                scope.Kind is AccessScopeKind.ManagerScoped or AccessScopeKind.CompanyWide
                || await _permissions.HasAsync(actorId, PermissionCodes.ViewScopedTasks)
                || await _permissions.HasAsync(actorId, PermissionCodes.ViewCompanyTasks);

            if (!canViewScoped)
                return null;

            var ids = await _scopeResolver
                .FilterEmployees(
                    _employeeRepo.GetAll(e => e.CompanyId == scope.CompanyId && e.IsActive),
                    scope)
                .Select(e => e.Id)
                .ToListAsync();

            return ids.ToHashSet();
        }

        private async Task EnsureCanApproveLeaveAsync(int managerId, int leaveEmployeeId)
        {
            if (!await _permissions.HasAsync(managerId, PermissionCodes.ApproveLeave))
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            await EnsureCanSeeLeaveEmployeeForReviewAsync(managerId, leaveEmployeeId);
        }

        private async Task EnsureCanRejectLeaveAsync(int managerId, int leaveEmployeeId)
        {
            if (!await _permissions.HasAsync(managerId, PermissionCodes.RejectLeave))
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            await EnsureCanSeeLeaveEmployeeForReviewAsync(managerId, leaveEmployeeId);
        }

        /// <summary>
        /// Visibility for review: notification listeners only (same as leave list).
        /// Self-review only when company-wide view perms exist (legacy admin).
        /// </summary>
        private async Task EnsureCanSeeLeaveEmployeeForReviewAsync(int managerId, int leaveEmployeeId)
        {
            if (leaveEmployeeId == managerId)
            {
                var canSelfReview = await _permissions.HasAnyAsync(
                    managerId,
                    PermissionCodes.ViewCompanyReports,
                    PermissionCodes.ViewCompanyTasks);
                if (!canSelfReview)
                    throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);
                return;
            }

            var visibleIds = await GetLeaveVisibleEmployeeIdsAsync(managerId);
            if (!visibleIds.Contains(leaveEmployeeId))
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);
        }

        /// <summary>
        /// Leave list for reviewers = RoleNotificationSource subjects only (استقبال إشعارات من),
        /// same graph as leave notifications — not access-scope / all branch staff.
        /// </summary>
        private async Task<List<int>> GetLeaveVisibleEmployeeIdsAsync(int viewerId)
        {
            var fromListeners = await _orgManagers.GetListenableSubjectIdsAsync(viewerId);
            return fromListeners
                .Append(viewerId)
                .Distinct()
                .ToList();
        }

        private async Task EnsureLeaveCalendarEventAsync(Leave leave, int managerId)
        {
            var marker = $"{LeaveCalendarDescriptionPrefix}{leave.Id}";
            var exists = await _calendarEventRepo.GetAll(e =>
                    !e.IsDeleted &&
                    e.EventType == CalendarEventType.Leave &&
                    e.Description != null &&
                    e.Description.Contains(marker))
                .AnyAsync();

            if (exists)
                return;

            var leaveTypeName = leave.LeaveType?.NameEn
                ?? leave.LeaveType?.NameAr
                ?? "Leave";
            var employeeName = leave.Employee?.FullName ?? "Employee";

            var calendarEvent = new CalendarEvent
            {
                CompanyId = leave.Employee?.CompanyId,
                Title = $"{employeeName} - {leaveTypeName}",
                Description = marker,
                StartDate = leave.StartDate.Date,
                EndDate = leave.EndDate.Date.AddDays(1), // exclusive end for all-day ranges
                AllDay = true,
                EventType = CalendarEventType.Leave,
                Public = true,
                CreatedByEmployeeId = managerId,
                reminder = 0
            };

            await _calendarEventRepo.AddAsync(calendarEvent);
            await _calendarEventRepo.SaveChangesAsync();
        }
    }

}
