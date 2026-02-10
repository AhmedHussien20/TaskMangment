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
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly IRepository<Leave> _leaveRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<LeaveType> _leaveTypeRepo;
        private readonly IMapper _mapper;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IRepository<Employee> _empRepo;
        private readonly IUserAccessContextProvider _accessProvider;
        private readonly IGetHigherManager _getHigherManager;


        public LeaveService(
            IRepository<Leave> leaveRepo,
            IRepository<Employee> employeeRepo,
            IRepository<LeaveType> leaveTypeRepo,
            IMapper mapper,
            IDomainEventDispatcher eventDispatcher,
            IRepository<Employee> empRepo,
            IUserAccessContextProvider accessProvider,
            IGetHigherManager getHigherManager

            )
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _mapper = mapper;
            _eventDispatcher = eventDispatcher;
            _empRepo = empRepo;
            _accessProvider = accessProvider;
            _getHigherManager = getHigherManager;
        }
         
        public async Task<ApiResponse<LeaveGetDto>> CreateAsync(LeaveAddDto dto, int employeeId)
        {
            if (!await _employeeRepo.IsExistAsync(employeeId))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound);

            if (!await _leaveTypeRepo.IsExistAsync(dto.LeaveTypeId))
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (dto.EndDate < dto.StartDate)
                throw new AppException(ErrorCodes.Invalid, StatusCodes.Status400BadRequest);

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

            var managerId = await _getHigherManager.GetDirectHigherManagerIdAsync(employeeId);

            //var managerId = full.Employee?.Branch?.ManagerID;

             if (!managerId.HasValue)
            {
                throw new AppException(ErrorCodes.Invalid, StatusCodes.Status400BadRequest);
            }


            await _eventDispatcher.PublishAsync(new LeaveEvent(
                                                  leave.Id,
                                                  managerId.Value,
                                                  full.Employee.FullName,
                                                  full.LeaveType.NameAr,
                                                  leave.StartDate,
                                                  leave.EndDate
                                                ));

            var result = _mapper.Map<LeaveGetDto>(full);

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

            if (roleLevel < 60)
            {
                query = query.Where(l => l.EmployeeId == employeeId);
            }
            else
            {
                var access = await _accessProvider.GetAsync(employeeId);

                var companyId = await _employeeRepo.GetAll(e => e.Id == employeeId)
                    .Select(e => e.CompanyId)
                    .FirstAsync();

                IQueryable<Employee> scopedEmployeesQuery = _employeeRepo
                    .GetAll(e => e.CompanyId == companyId && e.IsActive)
                    .ApplyAccessScope(access);

                if (roleLevel != 100)
                    scopedEmployeesQuery = scopedEmployeesQuery.ApplyRoleHierarchy(roleLevel);

                var scopedEmployeeIds = scopedEmployeesQuery.Select(e => e.Id);

                query = query.Where(l => scopedEmployeeIds.Contains(l.EmployeeId));

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

            var response = new PagedResponse<LeaveGetDto>(
                dtos,
                totalCount,
                request.PageIndex,
                request.PageSize);

            return ApiResponse<PagedResponse<LeaveGetDto>>.Ok(response);
        }


        public async Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetPendingForApprovalAsync(int managerId, LeaveRequest request)
        {

            IQueryable<Leave> query = _leaveRepo.GetAll(l => l.Status == LeaveStatus.Pending)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType);

            var totalCount = await query.CountAsync();

            query = query.OrderBy(l => l.CreatedDate);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<LeaveGetDto>>(list);

            var response = new PagedResponse<LeaveGetDto>(
                dtos, totalCount, request.PageIndex, request.PageSize);

            return ApiResponse<PagedResponse<LeaveGetDto>>.Ok(response);
        }

        // =========================
        // Approve
        // =========================
        public async Task<ApiResponse<bool>> ApproveAsync(int leaveId, int managerId, string managerFullName)
        {
            var leave = await _leaveRepo.GetAll(l => l.Id == leaveId)
                .Include(l => l.Employee)
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



            var managerLevel = await GetEmployeeRoleLevelAsync(managerId);

            if (leave.EmployeeId == managerId && managerLevel != 100)
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            var employeeLevel = await GetEmployeeRoleLevelAsync(leave.EmployeeId);

            if (managerLevel != 100 && employeeLevel > managerLevel)
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            leave.Status = LeaveStatus.Approved;
            leave.ApprovedById = managerId;
            leave.ApprovedAt = DateTime.UtcNow;

            await _leaveRepo.SaveChangesAsync();

            await _eventDispatcher.PublishAsync(new LeaveApprovedEvent(
                leave.Id,
                leave.EmployeeId,
                leave.Employee.FullName,
                managerFullName,
                leave.LeaveType.NameAr,
                leave.StartDate,
                leave.EndDate
            ));

            return ApiResponse<bool>.Ok(true, "Leave approved");
        }



        public async Task<ApiResponse<bool>> RejectAsync(int leaveId, int managerId, RejectLeaveDto rejectLeaveDto)
        {
            var leave = await _leaveRepo.GetAll(l => l.Id == leaveId)
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .Include(l => l.LeaveType)
                .FirstOrDefaultAsync();

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (leave.Status != LeaveStatus.Pending)
                throw new AppException(ErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(rejectLeaveDto.reason))
                throw new AppException(ErrorCodes.Invalid, StatusCodes.Status400BadRequest);

            var manager = await _empRepo.GetByIDAsync(managerId);
            if (manager == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var managerLevel = await GetEmployeeRoleLevelAsync(managerId);

            if (leave.EmployeeId == managerId && managerLevel != 100)
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            var employeeLevel = await GetEmployeeRoleLevelAsync(leave.EmployeeId);

            if (managerLevel != 100 && employeeLevel > managerLevel)
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);


            leave.Status = LeaveStatus.Rejected;
            leave.RejectionReason = rejectLeaveDto.reason;
            leave.ApprovedById = managerId;
            leave.ApprovedAt = DateTime.UtcNow;

            await _leaveRepo.SaveChangesAsync();

            await _eventDispatcher.PublishAsync(new LeaveRejectedEvent(
                leave.Id,
                leave.EmployeeId,
                leave.Employee.FullName,
                manager.FullName,
                leave.LeaveType.NameAr,
                leave.RejectionReason,
                leave.StartDate,
                leave.EndDate
            ));

            return ApiResponse<bool>.Ok(true, "Leave rejected");
        }


        public async Task<ApiResponse<LeaveGetDto>> GetByIdAsync(int leaveId)
        {
            var leave = await _leaveRepo.GetAll(l => l.Id == leaveId)
                                        .Include(l => l.Employee)
                                        .Include(l => l.LeaveType)
                                        .FirstOrDefaultAsync();

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var dto = _mapper.Map<LeaveGetDto>(leave);

            return ApiResponse<LeaveGetDto>.Ok(dto);
        }

        private async Task<int> GetEmployeeRoleLevelAsync(int employeeId)
        {
            var query = _empRepo.GetAll(e => e.Id == employeeId)
                .SelectMany(e => e.EmployeeRoles)
                .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
                .Select(er => (int?)er.Role.Level); 
            var maxLevel = await query.MaxAsync();

            return maxLevel ?? (int)RoleLevelEnum.Employee;
        }



    }

}
