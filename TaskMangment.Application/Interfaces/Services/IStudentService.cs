using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Student;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IStudentService
    {
        Task<ApiResponse<PagedResponse<StudentGetDto>>> GetAllAsync(StudentRequest request);
        Task<ApiResponse<StudentGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<StudentGetDto>> AddAsync(StudentAddEditDto dto);
        Task<ApiResponse<StudentGetDto>> UpdateAsync(int id, StudentAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
