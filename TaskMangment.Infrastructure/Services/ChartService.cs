using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs.ChartsDTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class ChartService: IChartService
    {
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<TaskPercentage> _taskPercentageRepo;


        public ChartService(IRepository<WorkTask> taskRepo, IRepository<TaskPercentage> taskPercentageRepo)
        {
            _taskRepo = taskRepo;
            _taskPercentageRepo = taskPercentageRepo;
        }
        public async Task<ApiResponse<EmpTasksChartResultDto>> GetEmployeeTasksChartAsync(
    int employeeId,
    DateTime fromDate,
    DateTime? toDate)
        {
            var endDate = toDate ?? DateTime.UtcNow;

            var lastPercentsQuery =
                _taskPercentageRepo.GetAll()
                    .GroupBy(p => p.TaskId)
                    .Select(g => new
                    {
                        TaskId = g.Key,
                        LatestPercent = g
                        .OrderByDescending(x => x.CreatedDate)
                       .Select(x => x.AchievementPercent)
                       .FirstOrDefault()

                    });

            var rows = await (
                from t in _taskRepo.GetAll()
                where t.Assignments.Any(a => a.IsActive && a.EmployeeId == employeeId)
                      && t.CreatedDate >= fromDate
                      && t.CreatedDate <= endDate
                join lp in lastPercentsQuery
                    on t.Id equals lp.TaskId into lpj
                from lp in lpj.DefaultIfEmpty()
                select new
                {
                    t.Id,
                    t.Title,
                    t.Status,
                    LatestPercent = lp == null ? null : lp.LatestPercent
                }
            )
            .AsNoTracking()
            .ToListAsync();

            if (rows.Count == 0)
                return ApiResponse<EmpTasksChartResultDto>.Ok(new EmpTasksChartResultDto());

            var tasks = rows
    .OrderBy(x => x.Title)
    .Select(x => new EmpTaskChartItem
    {
        TaskName = $"[{x.Id}] {x.Title}",
        Percent =
            x.Status == WorkTaskStatus.Closed
                ? "100%"
                : x.Status == WorkTaskStatus.AutoClose
                    ? "10%"
                    : string.IsNullOrWhiteSpace(x.LatestPercent)
                        ? "0%"
                        : x.LatestPercent
    })
    .ToList();

            var statusCounts = rows
                .GroupBy(x => x.Status)
                .Select(g => new TaskStatusCountDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var dto = new EmpTasksChartResultDto
            {
                Tasks = tasks,
                StatusCounts = statusCounts
            };


            return ApiResponse<EmpTasksChartResultDto>.Ok(dto);
        }


    }
}
