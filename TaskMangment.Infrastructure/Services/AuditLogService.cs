using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.AuditLogs;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditRepo;
        private readonly ICurrentUserService _currentUserService;

        public AuditLogService(
            IRepository<AuditLog> auditRepo,
            ICurrentUserService currentUserService)
        {
            _auditRepo = auditRepo;
            _currentUserService = currentUserService;
        }

        public async Task LogAsync(string entityName, int? entityId, string action, string details = null)
        {
            try
            {
                var audit = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    ChangedBy = _currentUserService.UserId,
                    ChangedAt = DateTime.UtcNow,
                    Details = details
                };

                await _auditRepo.AddAsync(audit);
                await _auditRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audit log error: {ex.Message}");
            }
        }
    }
}

