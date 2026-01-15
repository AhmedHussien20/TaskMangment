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
        public async Task<List<EmployeeCommentsActivityReportDto>> GetEmployeesCommentsActivityAsync(DateTime fromDate,DateTime toDate)
        {
            return await _context.TaskComments
                .Where(c => c.CreatedDate >= fromDate && c.CreatedDate <= toDate)
                .GroupBy(c => new
                {
                    c.EmployeeId,
                    c.Employee.FullName
                })
                .Select(g => new EmployeeCommentsActivityReportDto
                {
                    EmployeeName = g.Key.FullName,

                    TotalComments = g.Count(),

                    DistinctTasksCount = g.Select(x => x.TaskId).Distinct().Count(),

                    AvgCommentsPerTask = g.Select(x => x.TaskId).Distinct().Count() == 0
                        ? 0
                        : (decimal)g.Count() / g.Select(x => x.TaskId).Distinct().Count(),

                    LastCommentDate = g.Max(x => x.CreatedDate)
                })
                .OrderByDescending(x => x.TotalComments)
                .AsNoTracking()
                .ToListAsync();
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


        public async Task<List<EmployeeAssignmentsReportDto>> GetMostAssignedEmployeesAsync( DateTime? fromDate = null,DateTime? toDate = null)
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
                return new List<EmployeeAssignmentsReportDto>();

            DateTime today = DateTime.Today;
            DateTime closingSoonDate = today.AddDays(3);

            return await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    a.Employee.FullName
                })
                .Select(g => new EmployeeAssignmentsReportDto
                {
                    EmployeeName = g.Key.FullName,

                    TotalTasks = g.Count(),

                    NewTasks = g.Count(x => x.Task.Status == WorkTaskStatus.New),
                    InProgressTasks = g.Count(x => x.Task.Status == WorkTaskStatus.InProgress),
                    ClosedTasks = g.Count(x => x.Task.Status == WorkTaskStatus.Closed),

                    OverdueTasks = g.Count(x =>
                        x.Task.Status != WorkTaskStatus.Closed &&
                        x.Task.DueDate != null &&
                        x.Task.DueDate < today),

                    ClosingSoonTasks = g.Count(x =>
                        x.Task.Status != WorkTaskStatus.Closed &&
                        x.Task.DueDate != null &&
                        x.Task.DueDate >= today &&
                        x.Task.DueDate <= closingSoonDate),

                    CompletionRate = g.Count(x => x.Task.Status == WorkTaskStatus.Closed) == 0
                        ? 0
                        : (decimal)g.Count(x => x.Task.Status == WorkTaskStatus.Closed) * 100 / g.Count()
                })
                .OrderByDescending(x => x.TotalTasks)
                .AsNoTracking()
                .ToListAsync();
        }




        public async Task<List<EmployeeOnTimeReportDto>> GetOnTimeCompletionReportAsync(string role,int employeeId,DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.TaskAssignments
                                .Include(a => a.Employee)
                                .Include(a => a.Task)
                                .AsQueryable();

            if (role != "Manager")
                query = query.Where(a => a.EmployeeId == employeeId);

            if (fromDate.HasValue)
                query = query.Where(a => a.Task.ClosedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Task.ClosedAt <= toDate.Value);

     
            query = query.Where(a =>
                a.Task.Status == WorkTaskStatus.Closed &&
                a.Task.ClosedAt != null &&
                a.Task.DueDate != null);

            var result = await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    EmployeeName = a.Employee.FullName
                })
                .Select(g => new EmployeeOnTimeReportDto
                {
                    EmployeeName = g.Key.EmployeeName,

                    TotalClosedTasks = g.Count(),

                    OnTimeTasks = g.Count(x =>
                        x.Task.ClosedAt <= x.Task.DueDate),

                    LateTasks = g.Count(x =>
                        x.Task.ClosedAt > x.Task.DueDate),

                    CommitmentPercentage =
                        g.Count() == 0
                        ? 0
                        : (decimal)g.Count(x => x.Task.ClosedAt <= x.Task.DueDate) * 100 / g.Count()
                })
                .Where(x => x.TotalClosedTasks > 0)
                .OrderByDescending(x => x.CommitmentPercentage)
                .ThenByDescending(x => x.TotalClosedTasks)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }




        public async Task<List<EmployeeArchivedTasksReportDto>> GetMostArchivedEmployeesAsync( DateTime? fromDate = null,DateTime? toDate = null)
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
                    EmployeeName = a.Employee.FullName
                })
                .Select(g => new EmployeeArchivedTasksReportDto
                {
                    EmployeeName = g.Key.EmployeeName,

                    TotalTasks = g.Count(),

                    ArchivedTasksCount = g.Count(x => x.Task.Status == WorkTaskStatus.Archived),

                    ArchiveRate = g.Count() == 0
                        ? 0
                        : (decimal)g.Count(x => x.Task.Status == WorkTaskStatus.Archived) * 100 / g.Count()
                })
                .Where(x => x.ArchivedTasksCount > 0)
                .OrderByDescending(x => x.ArchiveRate)
                .ThenByDescending(x => x.ArchivedTasksCount)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }
        public async Task<TaskDiscountAuditReportDto> GetTaskDiscountAuditReportAsync(TaskDiscountReportFilterDto dto)
        {
            IQueryable<TaskAssignment> query = _context.TaskAssignments
                .Include(a => a.Task)
                .Include(a => a.Task.AssignedBy)
                .Include(a => a.Employee);

            if (dto.MovementType == TaskMovementType.Incoming)
            {
                query = query.Where(a => a.EmployeeId == dto.EmployeeId);
            }
            else
            {
                query = query.Where(a => a.Task.AssignedByEmployeeId == dto.EmployeeId);
            }

            query = query.Where(a => a.Task.DueDate >= dto.FromDate);

            if (dto.ToDate.HasValue)
                query = query.Where(a => a.Task.DueDate <= dto.ToDate.Value);

            if (dto.Status.HasValue)
                query = query.Where(a => a.Task.Status == dto.Status.Value);

            query = query.Where(a => a.Task.Status != WorkTaskStatus.New);

            var flatRows = await query
                .Select(a => new TaskDiscountAuditRowDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    AssignedBy = a.Task.AssignedBy.FullName,
                    ClosedDate = a.Task.DueDate,

                    Status =
                        a.Task.Status == WorkTaskStatus.Archived ? "مؤرشفة" :
                        a.Task.Status == WorkTaskStatus.Closed ? "مغلقة" :
                        a.Task.Status == WorkTaskStatus.AutoClose ? "مغلقة تلقائيًا" :
                        a.Task.Status == WorkTaskStatus.InProgress ? "قيد التنفيذ" :
                        "غير محدد",

                    AutoDiscount = _context.Discounts
                        .Where(d => d.TaskId == a.TaskId && d.AutoDiscount)
                        .Sum(d => (decimal?)d.Amount) ?? 0,

                    ManualDiscount = _context.Discounts
                        .Where(d => d.TaskId == a.TaskId && !d.AutoDiscount)
                        .Sum(d => (decimal?)d.Amount) ?? 0,

                    EmployeeName =
                                dto.MovementType == TaskMovementType.Incoming
                                    ? (a.Employee != null ? a.Employee.FullName : "غير معروف")   
                                    : (a.Task.AssignedBy != null ? a.Task.AssignedBy.FullName : "غير معروف")  

                })
                .AsNoTracking()
                .ToListAsync();

            // ===== Group in Memory (Audit Safe) =====
            var groups = flatRows
                .GroupBy(x => x.EmployeeName)
                .Select(g => new EmployeeDiscountAuditGroupDto
                {
                    EmployeeName = g.Key,
                    Tasks = g.OrderBy(x => x.ClosedDate).ToList()
                })
                .OrderByDescending(g => g.TotalDiscount)
                .ToList();

            return new TaskDiscountAuditReportDto
            {
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                Groups = groups,
                MovementType = dto.MovementType
            };
        }


        public async Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync(TaskDiscountReportFilterDto dto)
        {
            IQueryable<TaskAssignment> query = _context.TaskAssignments
                .Include(a => a.Task)
                .Include(a => a.Task.AssignedBy)
                .Include(a => a.Employee) 
                .AsQueryable();

            if (dto.MovementType == TaskMovementType.Incoming)
            {
                query = query.Where(a => a.EmployeeId == dto.EmployeeId);
            }
            else
            {
                query = query.Where(a => a.Task.AssignedByEmployeeId == dto.EmployeeId);
            }

            query = query.Where(a => a.Task.DueDate >= dto.FromDate);
            if (dto.ToDate.HasValue)
                query = query.Where(a => a.Task.DueDate <= dto.ToDate.Value);

            if (dto.Status.HasValue)
                query = query.Where(a => a.Task.Status == dto.Status.Value);

            query = query.Where(a => a.Task.Status != WorkTaskStatus.New);

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
                    AutoDiscount = _context.Discounts.Where(d => d.TaskId == a.TaskId && d.AutoDiscount).Sum(d => (decimal?)d.Amount) ?? 0,
                    ManualDiscount = _context.Discounts.Where(d => d.TaskId == a.TaskId && !d.AutoDiscount).Sum(d => (decimal?)d.Amount) ?? 0,
                    Evaluation = a.Task.Status == WorkTaskStatus.Archived ? "جيد" :
                                 a.Task.Status == WorkTaskStatus.Closed ? "جيد" :
                                 a.Task.Status == WorkTaskStatus.AutoClose ? "سيئ" :
                                 "قيد التنفيذ",
                    EmployeeName = a.Employee != null ? a.Employee.FullName : "غير معروف"
                })
                .OrderBy(r => r.EmployeeName)
                .ThenBy(r => r.ClosedDate)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<List<TaskActivityReportDto>> GetTaskActivityReportAsync(string role, int employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.TaskComments
                .Include(c => c.Employee)
                .Include(c => c.Task)
                .ThenInclude(t => t.AssignedBy)
                .AsQueryable();

            if (role != "Manager")
            {
                query = query.Where(c => c.Task.Assignments.Any(a => a.EmployeeId == employeeId));
            }

            if (fromDate.HasValue)
                query = query.Where(c => c.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(c => c.CreatedDate <= toDate.Value);
            if (!fromDate.HasValue && !toDate.HasValue)

                return new List<TaskActivityReportDto>();

            var result = await query
                .Select(c => new TaskActivityReportDto
                {
                    TaskTitleWithId = $"[{c.Task.Id}] {c.Task.Title}",
                    AssignedBy = c.Task.AssignedBy != null ? c.Task.AssignedBy.FullName : "غير معروف",
                    Comment = c.CommentText,
                    CommentDate = c.CreatedDate,
                    CommentedBy = c.Employee != null ? c.Employee.FullName : "غير معروف"
                })
                .OrderBy(c => c.CommentDate)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


        public async Task<List<TaskMovementReportDto>> GetTaskMovementReportAsync(TaskMovementReportFilterDto dto)
        {
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var query = _context.TaskComments
                .Include(c => c.Employee)
                .Include(c => c.Task)
                .ThenInclude(t => t.AssignedBy)
                .AsQueryable();

            query = query.Where(c =>
                c.CreatedDate >= todayStart && c.CreatedDate < todayEnd);

            // ===== صادرة =====
            if (dto.MovementType == TaskMovementType.Outgoing)
            {
                query = query.Where(c =>
                    c.Task.AssignedByEmployeeId == dto.EmployeeId);
            }

            // ===== واردة =====
            else if (dto.MovementType == TaskMovementType.Incoming)
            {
                // الموظف = اللي علّق
                query = query.Where(c =>
                    c.EmployeeId == dto.EmployeeId);
            }

            var result = await query
                .Select(c => new TaskMovementReportDto
                {
                    ReportTitle = dto.ReportTitle,

                    TaskTitleWithId = $"[{c.Task.Id}] {c.Task.Title}",
                    MovementType = dto.MovementType,

                    AssignedBy = c.Task.AssignedBy != null
                        ? c.Task.AssignedBy.FullName
                        : "غير معروف",

                    CommentedBy = c.Employee != null
                        ? c.Employee.FullName
                        : "غير معروف",

                    Comment = c.CommentText,
                    CommentDate = c.CreatedDate
                })
                .OrderBy(x => x.CommentDate)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<List<TasksClosingSoonDto>> GetTasksClosingSoonAsync(int employeeId, DateTime fromDate,DateTime toDate)
        {
            var query = _context.TaskAssignments.Include(a => a.Task).ThenInclude(t => t.AssignedBy)
                                .Include(a => a.Employee)
                                    .ThenInclude(e => e.Branch)
                                        .ThenInclude(b => b.Area)
                                .Include(a => a.Employee)
                                    .ThenInclude(e => e.Company)
                                .Where(a =>
                                    a.EmployeeId == employeeId &&
                                    (a.Task.Status == WorkTaskStatus.New ||
                                     a.Task.Status == WorkTaskStatus.InProgress) &&
                                    a.Task.DueDate != null &&
                                    a.Task.DueDate >= fromDate &&
                                    a.Task.DueDate <= toDate
                                );

            var result = await query
                .OrderBy(a => a.Task.DueDate)
                .Select(a => new TasksClosingSoonDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    Status = a.Task.Status,
                    AssignedBy = a.Task.AssignedBy != null
                        ? a.Task.AssignedBy.FullName
                        : "غير معروف",

                    DueDate = a.Task.DueDate,

                    EmployeeName = a.Employee != null
                        ? a.Employee.FullName
                        : "غير معروف",

                    CompanyName = a.Employee.Company != null
                        ? a.Employee.Company.Name
                        : "-",

                    BranchName = a.Employee.Branch != null
                        ? a.Employee.Branch.Name
                        : "-",

                    AreaName = a.Employee.Branch != null && a.Employee.Branch.Area != null
                        ? a.Employee.Branch.Area.Name
                        : "-"
                })
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


    }

}
