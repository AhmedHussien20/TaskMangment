using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Job;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IJobService
    {
        Task<ApiResponse<PagedResponse<JobGetDto>>> GetAllAsync(JobRequest request);
        Task<ApiResponse<JobGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> AddAsync(JobAddEditDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, JobAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
