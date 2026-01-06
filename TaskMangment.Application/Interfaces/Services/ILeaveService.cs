using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ILeaveService
    {
        Task<ApiResponse<LeaveGetDto>> CreateAsync(LeaveAddDto dto, int employeeId);

        Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetMyRequestsAsync(int employeeId, BaseApiRequest request);

        Task<ApiResponse<PagedResponse<LeaveGetDto>>> GetPendingForApprovalAsync(int managerId, BaseApiRequest request);

        Task<ApiResponse<bool>> ApproveAsync(int leaveId, int managerId);

        Task<ApiResponse<bool>> RejectAsync(int leaveId, int managerId, string reason);
    }


}
