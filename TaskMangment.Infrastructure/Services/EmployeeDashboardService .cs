using Microsoft.EntityFrameworkCore; 
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Dashboards.Employee;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class EmployeeDashboardService : IEmployeeDashboardService
    {
        private readonly IRepository<TaskAssignment> _assignmentRepo;
        private readonly IRepository<Warning> _warningRepo;
        private readonly IRepository<Deduction> _deductionRepo;
        private readonly ICachingService _cache;

        public EmployeeDashboardService(
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<Warning> warningRepo,
            IRepository<Deduction> deductionRepo,
            ICachingService cache)
        {
            _assignmentRepo = assignmentRepo;
            _warningRepo = warningRepo;
            _deductionRepo = deductionRepo;
            _cache = cache;
        }

        public async Task<ApiResponse<EmployeeDashboardDto>> GetDashboardAsync(int employeeId)
        {
            string cacheKey = $"dashboard:employee:{employeeId}";

            var cached = await _cache.GetAsync<EmployeeDashboardDto>(cacheKey);
            if (cached != null)
                return ApiResponse<EmployeeDashboardDto>.Ok(cached);

            var now = DateTime.UtcNow;
            var dueSoonDate = now.AddDays(2);

            var activeTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.IsActive &&
                !a.IsClosed);

            var dueSoonTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.IsActive &&
                !a.IsClosed &&
                a.Task.DueDate != null &&
                a.Task.DueDate <= dueSoonDate);

            var warningsCount = await _warningRepo.CountAsync(w =>
                w.TaskAssignment.EmployeeId == employeeId);

            var penaltiesTotal = await _deductionRepo
                .GetAll(d => d.EmployeeId == employeeId)
                .SumAsync(d => d.Amount);

            var myTasks = await _assignmentRepo
                .GetAll(a =>
                    a.EmployeeId == employeeId &&
                    a.IsActive)
                .Select(a => new MyTaskDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    Status = a.Task.Status,
                    DueDate = a.Task.DueDate,
                    ProgressPercent = a.ProgressPercent
                })
                .OrderBy(a => a.DueDate)
                .Take(10)
                .ToListAsync();

            var completedTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.IsClosed);

            var totalTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId);

            var dto = new EmployeeDashboardDto
            {
                Kpis = new EmployeeKpiDto
                {
                    MyActiveTasks = activeTasks,
                    DueSoonTasks = dueSoonTasks,
                    MyWarnings = warningsCount,
                    MyPenalties = penaltiesTotal
                },
                MyTasks = myTasks,
                Performance = new PerformanceSummaryDto
                {
                    CompletedTasks = completedTasks,
                    TotalTasks = totalTasks
                }
            };

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(3));

            return ApiResponse<EmployeeDashboardDto>.Ok(dto);
        }
    }
}

