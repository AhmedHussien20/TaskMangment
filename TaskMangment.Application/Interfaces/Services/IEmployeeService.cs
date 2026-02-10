using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<ApiResponse<PagedResponse<EmployeeGetDto>>> GetAllAsync(EmployeeRequest request, int employeeId,int roleLevel);
        Task<ApiResponse<EmployeeGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<EmployeeGetDto>> AddAsync(EmployeeAddEditDto dto, int CampanyId);
        Task<ApiResponse<EmployeeGetDto>> UpdateAsync(int id, EmployeeAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id); // soft delete
        Task<ApiResponse<List<FunctionCodeEnumDto>>> GetFunctionCodesAsync();

    }
}
