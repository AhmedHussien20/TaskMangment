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
        Task<List<EmployeeCommentsReportDto>> GetTopEmployeesByCommentsAsync(int currentEmployeeId, DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<EmployeeAssignmentsReportDto>> GetMostAssignedEmployeesAsync(int currentEmployeeId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<EmployeeOnTimeReportDto>> GetOnTimeCompletionReportAsync(int employeeId, DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<EmployeeArchivedTasksReportDto>> GetMostArchivedEmployeesAsync(int currentEmployeeId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync( int currentEmployeeId, TaskDiscountReportFilterDto dto);
        Task<List<TaskActivityReportDto>> GetTaskActivityReportAsync(int employeeId, ExportType exportType, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TaskMovementReportDto>> GetTaskMovementReportAsync(int currentEmployeeId, TaskMovementReportFilterDto dto, ExportType exportTypee);
        Task<List<TasksClosingSoonDto>> GetTasksClosingSoonAsync(int currentEmployeeId, int? employeeId, DateTime fromDate, DateTime toDate);
        Task<List<EmployeeCommentsActivityReportDto>> GetEmployeesCommentsActivityAsync(int currentEmployeeId, DateTime? fromDate, DateTime? toDate);
        Task<TaskDiscountAuditReportDto> GetTaskDiscountAuditReportAsync(int currentEmployeeId, TaskDiscountReportFilterDto dto);
    }
}
