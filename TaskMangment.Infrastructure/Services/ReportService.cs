using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeeCommentsReportDto>> GetTopEmployeesByCommentsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.TaskComments.Include(c => c.Employee).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(c => c.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(c => c.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeCommentsReportDto>();

            return await query.GroupBy(c => new
                {
                    c.EmployeeId,
                    c.Employee.FullName
                }).Select(g => new EmployeeCommentsReportDto
                {
                    EmployeeName = g.Key.FullName,
                    CommentsCount = g.Count()
                }) .OrderByDescending(x => x.CommentsCount)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<List<EmployeeAssignmentsReportDto>> GetMostAssignedEmployeesAsync(DateTime? fromDate = null,DateTime? toDate = null)
        {
            var query = _context.TaskAssignments.Include(a => a.Employee).Include(a => a.Task).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeAssignmentsReportDto>();

            return await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    a.Employee.FullName
                }).Select(g => new EmployeeAssignmentsReportDto
                {
                    EmployeeName = g.Key.FullName,
                    TasksCount = g.Count()
                }).OrderByDescending(x => x.TasksCount) .AsNoTracking().ToListAsync();
        }



        public async Task<List<EmployeeOnTimeReportDto>> GetOnTimeCompletionReportAsync(DateTime? fromDate = null,DateTime? toDate = null)
        {
            var query = _context.TaskAssignments.Include(a => a.Employee).Include(a => a.Task).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeOnTimeReportDto>();

            var result = await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    a.Employee.FullName
                })
                .Select(g => new EmployeeOnTimeReportDto
                {
                    EmployeeName = g.Key.FullName,

                    TotalTasks = g.Count(),

                    OnTimeTasks = g.Count(x =>
                        x.Task.Status == WorkTaskStatus.Closed &&
                        x.Task.DueDate >= DateTime.UtcNow)
                })
                .Where(x => x.OnTimeTasks > 0)
                .OrderByDescending(x => x.OnTimeTasks)
                .ThenByDescending(x => x.TotalTasks)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


        public async Task<List<EmployeeArchivedTasksReportDto>> GetMostArchivedEmployeesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.TaskAssignments
                .Include(a => a.Employee)
                .Include(a => a.Task)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeArchivedTasksReportDto>();

            var result = await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    a.Employee.FullName
                })
                .Select(g => new EmployeeArchivedTasksReportDto
                {
                    EmployeeName = g.Key.FullName,
                    ArchivedTasksCount = g.Count(x => x.Task.Status == WorkTaskStatus.Archived)
                })
                .Where(x => x.ArchivedTasksCount > 0) 
                .OrderByDescending(x => x.ArchivedTasksCount)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync(TaskDiscountReportFilterDto dto)
        {
            var employee = await _context.Employees
       .Where(e => e.Id == dto.EmployeeId)
       .Select(e => new { e.FullName })
       .FirstOrDefaultAsync();


            var query = _context.TaskAssignments
                .Include(a => a.Task)
                .Include(a => a.Task.AssignedBy)
                .AsQueryable();

            // ===== Mandatory Filters =====

            query = query.Where(a => a.EmployeeId == dto.EmployeeId);
            query = query.Where(a => a.Task.DueDate >= dto.FromDate);


            // ===== Optional Filters =====

            if (dto.Status.HasValue)
                query = query.Where(a => a.Task.Status == dto.Status.Value);

            if (dto.ToDate.HasValue)
                query = query.Where(a => a.Task.DueDate <= dto.ToDate.Value);

            query = query.Where(a => a.Task.Status != WorkTaskStatus.New);

            // ===== Projection =====

            var result = await query
                .Select(a => new TaskDiscountReportDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    AssignedBy = a.Task.AssignedBy.FullName,
                    ClosedDate = a.Task.DueDate,

                    Status = a.Task.Status == WorkTaskStatus.Archived ? "مورشف" :
                  a.Task.Status == WorkTaskStatus.Closed ? "مغلقة" :
                  a.Task.Status == WorkTaskStatus.AutoClose ? "مغلق تلقائي" :
                  a.Task.Status == WorkTaskStatus.InProgress ? "قيد التنفيذ" : "غير محدد",

                    AutoDiscount = _context.Discounts
                        .Where(d => d.TaskId == a.TaskId && d.AutoDiscount)
                        .Sum(d => (decimal?)d.Amount) ?? 0,

                    ManualDiscount = _context.Discounts
                        .Where(d => d.TaskId == a.TaskId && !d.AutoDiscount)
                        .Sum(d => (decimal?)d.Amount) ?? 0,

                    Evaluation =
                        a.Task.Status == WorkTaskStatus.Archived ? "جيد" :
                        a.Task.Status == WorkTaskStatus.Closed ? "جيد" :
                        a.Task.Status == WorkTaskStatus.AutoClose ? "سيئ" :
                        "قيد التنفيذ",

                   EmployeeName = employee != null ? employee.FullName : "غير معروف"

                })
                .AsNoTracking()
                .ToListAsync();

            return result;
        }



    }

}
