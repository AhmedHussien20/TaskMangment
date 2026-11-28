using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IEmployeeTypeService
    {
        Task<ApiResponse<PagedResponse<EmployeeTypeGetDto>>> GetAllAsync(EmployeeTypeRequest request);
        Task<ApiResponse<List<FunctionCodeEnumDto>>> GetLookupAsync();
        Task<ApiResponse<EmployeeTypeGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<EmployeeTypeGetDto>> AddAsync(EmployeeTypeAddEditDto dto);
        Task<ApiResponse<EmployeeTypeGetDto>> UpdateAsync(int id, EmployeeTypeAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
