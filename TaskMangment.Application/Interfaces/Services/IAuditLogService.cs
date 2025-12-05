using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.AuditLogs;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string entityName, int? entityId, string action, string details = null);
        Task<ApiResponse<PagedResponse<AuditLogDTO>>> GetAllAsync(AuditLogRequest request);
        Task<ApiResponse<AuditLogDTO>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> DeleteAsync(int id); 

    }
}
