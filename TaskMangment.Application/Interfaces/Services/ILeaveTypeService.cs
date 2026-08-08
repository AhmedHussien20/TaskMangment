using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ILeaveTypeService
    {
        Task<ApiResponse<List<LeaveTypeGetDto>>> GetAllAsync();
        Task<ApiResponse<LeaveTypeGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<LeaveTypeGetDto>> AddAsync(LeaveTypeAddEditDto dto, int companyId, int createdBy);
        Task<ApiResponse<LeaveTypeGetDto>> UpdateAsync(int id, LeaveTypeAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }


}
