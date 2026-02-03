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
        Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync(int roleLevel, int currentEmployeeId, TaskDiscountReportFilterDto dto);
        Task<List<TaskActivityReportDto>> GetTaskActivityReportAsync(int roleLevel, int employeeId, ExportType exportType, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TaskMovementReportDto>> GetTaskMovementReportAsync(TaskMovementReportFilterDto dto, ExportType exportType);
        Task<List<TasksClosingSoonDto>> GetTasksClosingSoonAsync(int roleLevel, int currentEmployeeId, int? employeeId, DateTime fromDate, DateTime toDate);
        Task<List<EmployeeCommentsActivityReportDto>> GetEmployeesCommentsActivityAsync(DateTime? fromDate, DateTime? toDate);
        Task<TaskDiscountAuditReportDto> GetTaskDiscountAuditReportAsync(int roleLevel,int currentEmployeeId, TaskDiscountReportFilterDto dto);
    }
}
