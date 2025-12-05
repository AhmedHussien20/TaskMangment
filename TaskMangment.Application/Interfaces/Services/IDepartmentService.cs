using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Department;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IDepartmentService
    {
        Task<ApiResponse<PagedResponse<DepartmentGetDto>>> GetAllAsync(DepartmentRequest request);
        Task<ApiResponse<DepartmentGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<DepartmentGetDto>> AddAsync(DepartmentAddEditDto dto);
        Task<ApiResponse<DepartmentGetDto>> UpdateAsync(int id, DepartmentAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
