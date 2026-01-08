using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<List<EmployeeCommentsReportDto>> GetTopEmployeesByCommentsAsync( DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<EmployeeAssignmentsReportDto>> GetMostAssignedEmployeesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<EmployeeOnTimeReportDto>> GetOnTimeCompletionReportAsync(DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<EmployeeArchivedTasksReportDto>> GetMostArchivedEmployeesAsync( DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync(TaskDiscountReportFilterDto dto);

    }
}
