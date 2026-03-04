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
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string entityName, int? entityId, string action, string details = null);
        Task<List<AuditLog>> GetAllAsync();
        Task<AuditLog> GetByIdAsync(string id);
        Task DeleteAsync(string id);
    }
}
