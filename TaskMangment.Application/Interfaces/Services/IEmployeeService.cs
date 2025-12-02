using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<ApiResponse<ICollection<EmployeeGetDto>>> GetAllAsync();
        Task<ApiResponse<EmployeeGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> AddAsync(EmployeeAddEditDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, EmployeeAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
