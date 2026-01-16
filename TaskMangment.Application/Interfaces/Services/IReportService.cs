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
        Task<List<EmployeeOnTimeReportDto>> GetOnTimeCompletionReportAsync(string role, int employeeId, DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<EmployeeArchivedTasksReportDto>> GetMostArchivedEmployeesAsync( DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync(TaskDiscountReportFilterDto dto);
        Task<List<TaskActivityReportDto>> GetTaskActivityReportAsync(string role, int employeeId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TaskMovementReportDto>> GetTaskMovementReportAsync(TaskMovementReportFilterDto dto);
        Task<List<TasksClosingSoonDto>> GetTasksClosingSoonAsync(int employeeId, DateTime fromDate, DateTime toDate);
        Task<List<EmployeeCommentsActivityReportDto>> GetEmployeesCommentsActivityAsync(DateTime? fromDate, DateTime? toDate);

        Task<TaskDiscountAuditReportDto> GetTaskDiscountAuditReportAsync(TaskDiscountReportFilterDto dto);
    }
}
