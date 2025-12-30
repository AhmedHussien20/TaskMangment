using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Course;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ICourseService
    {
        Task<ApiResponse<PagedResponse<CourseGetDto>>> GetAllAsync(CourseRequest request);
        Task<ApiResponse<CourseGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<ICollection<CourseSubjectDto>>> GetSubjectsByCourseAsync(int courseId);
        Task<ApiResponse<CourseGetDto>> AddAsync(CourseAddEditDto dto);
        Task<ApiResponse<CourseGetDto>> UpdateAsync(int id, CourseAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
