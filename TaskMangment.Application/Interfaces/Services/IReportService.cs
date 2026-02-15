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
        Task<List<EmployeeCommentsReportDto>> GetTopEmployeesByCommentsAsync(int currentEmployeeId, int roleLevel, DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<EmployeeAssignmentsReportDto>> GetMostAssignedEmployeesAsync(int currentEmployeeId, int roleLevel, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<EmployeeOnTimeReportDto>> GetOnTimeCompletionReportAsync(int employeeId, int roleLevel, DateTime? fromDate = null,DateTime? toDate = null);
        Task<List<EmployeeArchivedTasksReportDto>> GetMostArchivedEmployeesAsync(int currentEmployeeId, int roleLevel, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync( int currentEmployeeId, int roleLevel, TaskDiscountReportFilterDto dto);
        Task<List<TaskActivityReportDto>> GetTaskActivityReportAsync(int employeeId, int roleLevel, ExportType exportType, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<TaskMovementReportDto>> GetTaskMovementReportAsync(int currentEmployeeId, int roleLevel, TaskMovementReportFilterDto dto, ExportType exportTypee);
        Task<List<TasksClosingSoonDto>> GetTasksClosingSoonAsync(int currentEmployeeId, int roleLevel, int? employeeId, DateTime fromDate, DateTime toDate);
        Task<List<EmployeeCommentsActivityReportDto>> GetEmployeesCommentsActivityAsync(int currentEmployeeId, int roleLevel, DateTime? fromDate, DateTime? toDate);
        Task<TaskDiscountAuditReportDto> GetTaskDiscountAuditReportAsync(int currentEmployeeId, int roleLevel, TaskDiscountReportFilterDto dto);

        Task<List<EmployeeTaskTrackingReportDto>> GetEmployeeTaskTrackingAsync(int currentEmployeeId,int roleLevel,int? employeeId,DateTime fromDate,DateTime? toDate = null);
        Task<List<BranchTaskReportRowDto>> GetBranchTasksReportAsync(int currentEmployeeId,int roleLevel, BranchTasksReportFilterDto dto);


    }
}
