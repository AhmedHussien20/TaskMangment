using AutoMapper;
using Microsoft.AspNetCore.Http; 
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly IRepository<Leave> _leaveRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<LeaveType> _leaveTypeRepo;
        private readonly IMapper _mapper;

        public LeaveService(
            IRepository<Leave> leaveRepo,
            IRepository<Employee> employeeRepo,
            IRepository<LeaveType> leaveTypeRepo,
            IMapper mapper)
        {
            _leaveRepo = leaveRepo;
            _employeeRepo = employeeRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _mapper = mapper;
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
                .Include(l => l.LeaveType)
                .FirstAsync();

            var result = _mapper.Map<LeaveGetDto>(full);

            return ApiResponse<LeaveGetDto>.Ok(result, "Leave request submitted");
        }
         
        public async Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetMyRequestsAsync(int employeeId, BaseApiRequest request)
        {
            var query = _leaveRepo.GetAll(l => l.EmployeeId == employeeId)
                .Include(l => l.LeaveType)
                .Include(l => l.Employee);

            var totalCount = await query.CountAsync();

            //query = query.OrderByDescending(l => l.CreatedDate);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<LeaveGetDto>>(list);

            var response = new PagedResponse<LeaveGetDto>(
                dtos, totalCount, request.PageIndex, request.PageSize);

            return ApiResponse<PagedResponse<LeaveGetDto>>.Ok(response);
        }
         
        public async Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetPendingForApprovalAsync(int managerId, BaseApiRequest request)
        {

            var query = _leaveRepo.GetAll(l => l.Status == LeaveStatus.Pending)
                .Include(l => l.Employee)
                .Include(l => l.LeaveType);

            var totalCount = await query.CountAsync();

            //query = query.OrderBy(l => l.CreatedDate);

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
        public async Task<ApiResponse<bool>> ApproveAsync(int leaveId, int managerId)
        {
            var leave = await _leaveRepo.GetByIDAsync(leaveId);

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (leave.Status != LeaveStatus.Pending)
                throw new AppException(ErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest);

            leave.Status = LeaveStatus.Approved;
            leave.ApprovedById = managerId;
            leave.ApprovedAt = DateTime.UtcNow;

            await _leaveRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Leave approved");
        }

        // =========================
        // Reject
        // =========================
        public async Task<ApiResponse<bool>> RejectAsync(int leaveId, int managerId, string reason)
        {
            var leave = await _leaveRepo.GetByIDAsync(leaveId);

            if (leave == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (string.IsNullOrWhiteSpace(reason))
                throw new AppException(ErrorCodes.Invalid, StatusCodes.Status400BadRequest);

            leave.Status = LeaveStatus.Rejected;
            leave.RejectionReason = reason;
            leave.ApprovedById = managerId;
            leave.ApprovedAt = DateTime.UtcNow;

            await _leaveRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Leave rejected");
        }
    }

}
