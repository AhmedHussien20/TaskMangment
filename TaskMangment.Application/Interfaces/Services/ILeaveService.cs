using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Common.ApiRequests.Leave;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ILeaveService
    {
        Task<ApiResponse<LeaveGetDto>> CreateAsync(LeaveAddDto dto, int employeeId);
        Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetMyRequestsAsync(LeaveRequest request, string role, int employeeId);
        Task<ApiResponse<LeaveGetDto>> GetByIdAsync(int leaveId);

        Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetPendingForApprovalAsync(int managerId, LeaveRequest request);

        Task<ApiResponse<bool>> ApproveAsync(int leaveId, int managerId);

        Task<ApiResponse<bool>> RejectAsync(int leaveId, int managerId, string reason);
    }


}
