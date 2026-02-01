using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs.ChartsDTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface  IChartService
    {
        Task<ApiResponse<EmpTasksChartResultDto>> GetEmployeeTasksChartAsync(int employeeId, DateTime fromDate, DateTime? toDate = null);
    }
}
