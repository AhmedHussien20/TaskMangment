using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Dashboards.Employee;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class Employee360Service : IEmployee360Service
    {
        private readonly IAccessScopeResolver _accessScope;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeDashboardService _dashboardService;
        private readonly IEmployeePermissionService _permissionService;
        private readonly IOrgManagerResolver _orgManagerResolver;
        private readonly IUserAccessContextProvider _accessContextProvider;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<TaskAssignment> _assignmentRepo;
        private readonly IRepository<TaskComment> _commentRepo;
        private readonly IRepository<Warning> _warningRepo;
        private readonly IRepository<Discount> _discountRepo;
        private readonly IRepository<TaskPercentage> _percentageRepo;
        private readonly IRepository<TaskExtensionRequest> _extensionRepo;
        private readonly IRepository<TaskCloseRequest> _closeRequestRepo;
        private readonly IRepository<Leave> _leaveRepo;
        private readonly IRepository<LeaveType> _leaveTypeRepo;
        private readonly IRepository<EmailQueue> _emailQueueRepo;
        private readonly IRepository<EmailTemplate> _emailTemplateRepo;
        private readonly IRepository<Notification> _notificationRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<Attachment> _attachmentRepo;

        public Employee360Service(
            IAccessScopeResolver accessScope,
            IEmployeeService employeeService,
            IEmployeeDashboardService dashboardService,
            IEmployeePermissionService permissionService,
            IOrgManagerResolver orgManagerResolver,
            IUserAccessContextProvider accessContextProvider,
            IRepository<Employee> employeeRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<TaskComment> commentRepo,
            IRepository<Warning> warningRepo,
            IRepository<Discount> discountRepo,
            IRepository<TaskPercentage> percentageRepo,
            IRepository<TaskExtensionRequest> extensionRepo,
            IRepository<TaskCloseRequest> closeRequestRepo,
            IRepository<Leave> leaveRepo,
            IRepository<LeaveType> leaveTypeRepo,
            IRepository<EmailQueue> emailQueueRepo,
            IRepository<EmailTemplate> emailTemplateRepo,
            IRepository<Notification> notificationRepo,
            IRepository<WorkTask> taskRepo,
            IRepository<Attachment> attachmentRepo)
        {
            _accessScope = accessScope;
            _employeeService = employeeService;
            _dashboardService = dashboardService;
            _permissionService = permissionService;
            _orgManagerResolver = orgManagerResolver;
            _accessContextProvider = accessContextProvider;
            _employeeRepo = employeeRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _managerBranchesRepo = managerBranchesRepo;
            _assignmentRepo = assignmentRepo;
            _commentRepo = commentRepo;
            _warningRepo = warningRepo;
            _discountRepo = discountRepo;
            _percentageRepo = percentageRepo;
            _extensionRepo = extensionRepo;
            _closeRequestRepo = closeRequestRepo;
            _leaveRepo = leaveRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _emailQueueRepo = emailQueueRepo;
            _emailTemplateRepo = emailTemplateRepo;
            _notificationRepo = notificationRepo;
            _taskRepo = taskRepo;
            _attachmentRepo = attachmentRepo;
        }

        public async Task<ApiResponse<Employee360Dto>> Get360Async(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null)
        {
            await EnsureCanViewAsync(actorId, employeeId);
            var period = BuildPeriod(range);

            var profileResult = await _employeeService.GetByIdAsync(employeeId);
            if (!profileResult.Success || profileResult.Data == null)
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound);

            var hireDate = await _employeeRepo.GetAll(e => e.Id == employeeId)
                .Select(e => (DateTime?)e.CreatedDate)
                .FirstOrDefaultAsync();

            var typeInfo = await _employeeRepo.GetAll(e => e.Id == employeeId)
                .Select(e => new
                {
                    e.EmployeeTypeId,
                    NameEn = e.EmployeeType != null ? e.EmployeeType.NameEn : null,
                    NameAr = e.EmployeeType != null ? e.EmployeeType.NameAr : null,
                    Code = e.EmployeeType != null ? e.EmployeeType.Code : null
                })
                .FirstOrDefaultAsync();

            var p = profileResult.Data;
            var employeeTypeName = p.EmployeeTypeName
                ?? typeInfo?.NameEn
                ?? typeInfo?.NameAr
                ?? typeInfo?.Code;

            var profile = new Employee360ProfileDto
            {
                Id = p.Id,
                EmployeeCode = $"EMP-{p.Id}",
                FullName = p.FullName,
                Title = p.Title,
                BranchName = p.BranchName,
                JobName = p.JobName,
                DepartmentName = p.DepartmentName,
                Email = p.Email,
                Mobile = p.Mobile,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                EmployeeTypeId = p.EmployeeTypeId ?? typeInfo?.EmployeeTypeId ?? 0,
                EmployeeTypeName = employeeTypeName,
                LastLoginDate = p.LastLoginDate,
                HireDate = hireDate,
                Qualification = p.Qualification,
                Address = p.Address,
                Nationality = p.Nationality,
                IdentityNumber = p.IdentityNumber
            };

            var accessSummary = await BuildAccessSummaryAsync(employeeId);
            var reportingManagers = await GetReportingManagersAsync(employeeId);
            var kpis = await BuildKpisAsync(employeeId, period);
            var sidebar = await BuildSidebarAsync(employeeId, range);
            var charts = await BuildOverviewChartsAsync(employeeId, range);

            var dto = new Employee360Dto
            {
                Profile = profile,
                Roles = accessSummary.Roles,
                ManagerScope = accessSummary.ManagerScope,
                ReportingManagers = reportingManagers,
                DirectManager = reportingManagers.FirstOrDefault(),
                Kpis = kpis,
                Sidebar = sidebar,
                OverviewCharts = charts,
                CanCreateTask = await _accessScope.CanAssignAsync(actorId, employeeId),
                CanEditEmployee = await _permissionService.HasAsync(actorId, PermissionCodes.UpdateEmployee)
            };

            return ApiResponse<Employee360Dto>.Ok(dto);
        }

        public async Task<ApiResponse<Employee360AccessDto>> GetAccessAsync(int actorId, int employeeId)
        {
            await EnsureCanViewAsync(actorId, employeeId);
            return ApiResponse<Employee360AccessDto>.Ok(await BuildAccessAsync(employeeId));
        }

        public async Task<ApiResponse<Employee360PerformanceDto>> GetPerformanceAsync(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var period = BuildPeriod(range);
            var (from, to) = GetNormalizedRange(range);
            var warnings = await _dashboardService.GetWarningsAsync(employeeId, period);
            var now = DateTime.UtcNow;

            var lateQuery = _assignmentRepo.GetAll(a =>
                    a.EmployeeId == employeeId && a.IsActive && !a.IsClosed && !a.IsDeleted &&
                    a.Task.DueDate != null && a.Task.DueDate < now);
            if (from.HasValue)
                lateQuery = lateQuery.Where(a => a.Task.DueDate >= from.Value);
            if (to.HasValue)
                lateQuery = lateQuery.Where(a => a.Task.DueDate <= to.Value);

            var lateTasks = await lateQuery
                .OrderBy(a => a.Task.DueDate)
                .Take(20)
                .Select(a => new Employee360DeadlineDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    DueDate = a.Task.DueDate,
                    Status = a.Task.Status.ToString(),
                    Priority = a.Task.Priority.ToString()
                })
                .ToListAsync();

            var dto = new Employee360PerformanceDto
            {
                Kpis = await BuildKpisAsync(employeeId, period),
                Warnings = warnings.Data ?? new List<WarningDto>(),
                LateTasks = lateTasks,
                MonthlyTrend = (await BuildOverviewChartsAsync(employeeId, range)).MonthlyProductivity
            };

            return ApiResponse<Employee360PerformanceDto>.Ok(dto);
        }

        public async Task<ApiResponse<Employee360DiscountsDto>> GetDiscountsAsync(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var period = BuildPeriod(range);
            var discounts = await _dashboardService.GetDeductionsAsync(employeeId, period);
            var list = discounts.Data ?? new List<DeductionDto>();

            return ApiResponse<Employee360DiscountsDto>.Ok(new Employee360DiscountsDto
            {
                Discounts = list,
                TotalAmount = list.Sum(d => d.Amount)
            });
        }

        public async Task<ApiResponse<Employee360LeaveDto>> GetLeaveAsync(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var (from, to) = GetNormalizedRange(range);
            var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var leavesQuery = _leaveRepo.GetAll(l => l.EmployeeId == employeeId && !l.IsDeleted);
            if (from.HasValue)
                leavesQuery = leavesQuery.Where(l => l.StartDate >= from.Value || l.CreatedDate >= from.Value);
            if (to.HasValue)
                leavesQuery = leavesQuery.Where(l => l.StartDate <= to.Value || l.CreatedDate <= to.Value);

            var leaves = await leavesQuery
                .Include(l => l.LeaveType)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();

            // Prefer highest leave-type max for the year; default 21.
            var annualCap = await _leaveTypeRepo.GetAll(t => !t.IsDeleted && t.MaxDaysPerYear != null)
                .OrderByDescending(t => t.MaxDaysPerYear)
                .Select(t => (double?)t.MaxDaysPerYear)
                .FirstOrDefaultAsync() ?? 21;

            var approvedWindowStart = from ?? yearStart;
            var approvedThisYear = leaves
                .Where(l => l.Status == LeaveStatus.Approved && l.StartDate >= approvedWindowStart
                    && (!to.HasValue || l.StartDate <= to.Value))
                .Sum(l => (l.EndDate.Date - l.StartDate.Date).TotalDays + 1);

            var dto = new Employee360LeaveDto
            {
                AllowedDays = annualCap,
                UsedDays = approvedThisYear,
                LeaveBalance = Math.Max(0, annualCap - approvedThisYear),
                PendingCount = leaves.Count(l => l.Status == LeaveStatus.Pending),
                ApprovedCount = leaves.Count(l => l.Status == LeaveStatus.Approved),
                RejectedCount = leaves.Count(l => l.Status == LeaveStatus.Rejected),
                History = leaves.Select(l => new Employee360LeaveItemDto
                {
                    Id = l.Id,
                    LeaveTypeName = l.LeaveType?.NameAr ?? l.LeaveType?.NameEn ?? "",
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status.ToString(),
                    Notes = l.Notes,
                    CreatedDate = l.CreatedDate
                }).ToList()
            };

            return ApiResponse<Employee360LeaveDto>.Ok(dto);
        }

        public async Task<ApiResponse<PagedResponse<Employee360EmailItemDto>>> GetEmailsAsync(
            int actorId, int employeeId, Employee360PagedRequest request)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var query = _emailQueueRepo.GetAll(e => e.UserId == employeeId && !e.IsDeleted);
            var from = NormalizeFrom(request.From);
            var to = NormalizeTo(request.To);

            if (from.HasValue)
                query = query.Where(e => e.CreatedDate >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.CreatedDate <= to.Value);
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var statusKey = request.Status.Equals("Queued", StringComparison.OrdinalIgnoreCase)
                    ? nameof(EmailStatus.Pending)
                    : request.Status;
                if (Enum.TryParse<EmailStatus>(statusKey, true, out var status))
                    query = query.Where(e => e.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                var typeKey = request.Type.Trim();
                var matchingRefTypes = Enum.GetValues<ReferenceType>()
                    .Where(r => r.ToString().Contains(typeKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                query = query.Where(e =>
                    e.TemplateKey.Contains(typeKey) ||
                    matchingRefTypes.Contains(e.ReferenceType));
            }
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.Trim();
                var matchingTemplateKeys = await _emailTemplateRepo
                    .GetAll(t => !t.IsDeleted && t.SubjectTemplate.Contains(key))
                    .Select(t => t.Key)
                    .ToListAsync();

                var statusMatches = Enum.GetValues<EmailStatus>()
                    .Where(s => MapEmailStatus(s).Contains(key, StringComparison.OrdinalIgnoreCase)
                             || s.ToString().Contains(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var matchingRefTypes = Enum.GetValues<ReferenceType>()
                    .Where(r => r.ToString().Contains(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var providerMatch = "SMTP".Contains(key, StringComparison.OrdinalIgnoreCase);
                var idMatches = int.TryParse(key, out var idKey);

                query = query.Where(e =>
                    (idMatches && (e.Id == idKey || e.ReferenceId == idKey || e.RetryCount == idKey)) ||
                    e.ToEmail.Contains(key) ||
                    (e.Cc != null && e.Cc.Contains(key)) ||
                    (e.Bcc != null && e.Bcc.Contains(key)) ||
                    e.TemplateKey.Contains(key) ||
                    matchingRefTypes.Contains(e.ReferenceType) ||
                    (e.ErrorMessage != null && e.ErrorMessage.Contains(key)) ||
                    (providerMatch && key.Equals("SMTP", StringComparison.OrdinalIgnoreCase)) ||
                    matchingTemplateKeys.Contains(e.TemplateKey) ||
                    statusMatches.Contains(e.Status));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(e => e.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var templateKeys = rows.Select(r => r.TemplateKey).Distinct().ToList();
            var subjects = await _emailTemplateRepo.GetAll(t => templateKeys.Contains(t.Key) && !t.IsDeleted)
                .Select(t => new { t.Key, t.SubjectTemplate })
                .ToListAsync();
            var subjectMap = subjects.ToDictionary(x => x.Key, x => x.SubjectTemplate, StringComparer.OrdinalIgnoreCase);

            var createdByIds = rows.Where(r => r.CreatedBy.HasValue).Select(r => r.CreatedBy!.Value).Distinct().ToList();
            var creators = await _employeeRepo.GetAll(e => createdByIds.Contains(e.Id))
                .Select(e => new { e.Id, e.FullName })
                .ToDictionaryAsync(e => e.Id, e => e.FullName);

            var emailTaskMap = await ResolveEmailTaskIdsAsync(rows);
            var taskIds = emailTaskMap.Values.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var taskTitles = taskIds.Count == 0
                ? new Dictionary<int, string>()
                : await _taskRepo.GetAll(t => taskIds.Contains(t.Id))
                    .Select(t => new { t.Id, t.Title })
                    .ToDictionaryAsync(t => t.Id, t => t.Title);

            var items = rows.Select(e =>
            {
                subjectMap.TryGetValue(e.TemplateKey, out var subject);
                // QueueDirectAsync stores "subject|||body" in ErrorMessage for some rows
                if (string.IsNullOrWhiteSpace(subject) && !string.IsNullOrWhiteSpace(e.ErrorMessage) && e.ErrorMessage.Contains("|||"))
                    subject = e.ErrorMessage.Split("|||", 2)[0];

                emailTaskMap.TryGetValue(e.Id, out var taskId);
                string? taskTitle = null;
                if (taskId.HasValue && taskTitles.TryGetValue(taskId.Value, out var title))
                    taskTitle = title;

                return new Employee360EmailItemDto
                {
                    Id = e.Id,
                    Date = e.CreatedDate,
                    Subject = subject ?? e.TemplateKey,
                    EmailType = e.TemplateKey,
                    Recipient = e.ToEmail,
                    Status = MapEmailStatus(e.Status),
                    DeliveryStatus = e.Status == EmailStatus.Sent ? "Delivered" : MapEmailStatus(e.Status),
                    Opened = false,
                    Clicked = false,
                    Retries = e.RetryCount,
                    Provider = "SMTP",
                    MessageId = e.Id.ToString(),
                    CreatedBy = e.CreatedBy.HasValue && creators.ContainsKey(e.CreatedBy.Value)
                        ? creators[e.CreatedBy.Value]
                        : null,
                    DeliveryTime = e.SentAt,
                    FailureReason = e.Status == EmailStatus.Failed ? e.ErrorMessage : null,
                    SmtpResponse = e.Status == EmailStatus.Failed ? e.ErrorMessage : null,
                    ProviderResponse = e.Status == EmailStatus.Failed ? e.ErrorMessage : null,
                    TaskId = taskId,
                    TaskTitle = taskTitle
                };
            }).ToList();

            return ApiResponse<PagedResponse<Employee360EmailItemDto>>.Ok(
                new PagedResponse<Employee360EmailItemDto>(items, total, pageIndex, pageSize));
        }

        public async Task<ApiResponse<PagedResponse<Employee360NotificationItemDto>>> GetNotificationsAsync(
            int actorId, int employeeId, Employee360PagedRequest request)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var query = _notificationRepo.GetAll(n => n.UserId == employeeId && !n.IsDeleted);
            var from = NormalizeFrom(request.From);
            var to = NormalizeTo(request.To);

            if (from.HasValue)
                query = query.Where(n => n.CreatedDate >= from.Value);
            if (to.HasValue)
                query = query.Where(n => n.CreatedDate <= to.Value);
            if (request.UnreadOnly == true)
                query = query.Where(n => !n.IsRead);
            if (!string.IsNullOrWhiteSpace(request.Type) &&
                Enum.TryParse<NotificationType>(request.Type, true, out var nType))
                query = query.Where(n => n.NotificationType == nType);
            if (!string.IsNullOrWhiteSpace(request.Channel) &&
                Enum.TryParse<NotificationChannel>(request.Channel, true, out var channel))
                query = query.Where(n => n.Channel == channel);
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var s = request.Status.Trim().ToLowerInvariant();
                if (s == "read") query = query.Where(n => n.IsRead);
                else if (s == "unread" || s == "sent" || s == "delivered") query = query.Where(n => !n.IsRead);
            }
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.Trim();
                var matchingTypes = Enum.GetValues<NotificationType>()
                    .Where(t => t.ToString().Contains(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var matchingChannels = Enum.GetValues<NotificationChannel>()
                    .Where(c => MapChannel(c).Contains(key, StringComparison.OrdinalIgnoreCase)
                             || c.ToString().Contains(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var matchingTaskIds = await _taskRepo
                    .GetAll(t => t.Title.Contains(key))
                    .Select(t => t.Id)
                    .Take(200)
                    .ToListAsync();

                var readMatch = "Read".Contains(key, StringComparison.OrdinalIgnoreCase);
                var unreadMatch = "Unread".Contains(key, StringComparison.OrdinalIgnoreCase)
                               || "Delivered".Contains(key, StringComparison.OrdinalIgnoreCase);

                query = query.Where(n =>
                    n.Id.ToString().Contains(key) ||
                    n.Message.Contains(key) ||
                    n.ReferenceId.ToString().Contains(key) ||
                    (n.TaskId != null && n.TaskId.ToString()!.Contains(key)) ||
                    matchingTypes.Contains(n.NotificationType) ||
                    matchingChannels.Contains(n.Channel) ||
                    (n.TaskId != null && matchingTaskIds.Contains(n.TaskId.Value)) ||
                    (n.ReferenceId > 0 && matchingTaskIds.Contains(n.ReferenceId)) ||
                    (readMatch && n.IsRead) ||
                    (unreadMatch && !n.IsRead));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(n => n.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.Id,
                    n.CreatedDate,
                    n.Message,
                    n.NotificationType,
                    n.Channel,
                    n.IsRead,
                    n.ModifiedDate,
                    n.TaskId,
                    n.ReferenceId
                })
                .ToListAsync();

            var taskIds = rows
                .Select(r => r.TaskId ?? (IsTaskNotification(r.NotificationType) && r.ReferenceId > 0 ? r.ReferenceId : (int?)null))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var taskTitles = taskIds.Count == 0
                ? new Dictionary<int, string>()
                : await _taskRepo.GetAll(t => taskIds.Contains(t.Id))
                    .Select(t => new { t.Id, t.Title })
                    .ToDictionaryAsync(t => t.Id, t => t.Title);

            var items = rows.Select(n =>
            {
                int? taskId = n.TaskId;
                if (!taskId.HasValue && IsTaskNotification(n.NotificationType) && n.ReferenceId > 0 &&
                    taskTitles.ContainsKey(n.ReferenceId))
                {
                    taskId = n.ReferenceId;
                }

                return new Employee360NotificationItemDto
                {
                    Id = n.Id,
                    Date = n.CreatedDate,
                    Title = n.Message,
                    Type = n.NotificationType.ToString(),
                    Priority = "Normal",
                    Channel = MapChannel(n.Channel),
                    Status = n.IsRead ? "Read" : "Delivered",
                    IsRead = n.IsRead,
                    ReadTime = n.IsRead ? n.ModifiedDate : null,
                    Delivered = true,
                    DeliveredTime = n.CreatedDate,
                    TaskId = taskId,
                    TaskTitle = taskId.HasValue && taskTitles.TryGetValue(taskId.Value, out var title) ? title : null
                };
            }).ToList();

            return ApiResponse<PagedResponse<Employee360NotificationItemDto>>.Ok(
                new PagedResponse<Employee360NotificationItemDto>(items, total, pageIndex, pageSize));
        }

        public async Task<ApiResponse<PagedResponse<Employee360CommentItemDto>>> GetCommentsAsync(
            int actorId, int employeeId, Employee360PagedRequest request)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var query = _commentRepo.GetAll(c => c.EmployeeId == employeeId && !c.IsDeleted);
            var from = NormalizeFrom(request.From);
            var to = NormalizeTo(request.To);

            if (from.HasValue)
                query = query.Where(c => c.CreatedDate >= from.Value);
            if (to.HasValue)
                query = query.Where(c => c.CreatedDate <= to.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.Trim();
                query = query.Where(c =>
                    (c.CommentText != null && c.CommentText.Contains(key)) ||
                    c.TaskId.ToString().Contains(key) ||
                    c.Task.Title.Contains(key));
            }

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.CreatedDate,
                    c.CommentText,
                    c.TaskId,
                    TaskTitle = c.Task.Title
                })
                .ToListAsync();

            var commentIds = rows.Select(r => r.Id).ToList();
            var attachmentCounts = commentIds.Count == 0
                ? new Dictionary<int, int>()
                : await _attachmentRepo
                    .GetAll(a =>
                        commentIds.Contains(a.ReferenceId) &&
                        a.AttachmentType == AttachmentType.Comment &&
                        !a.IsDeleted)
                    .GroupBy(a => a.ReferenceId)
                    .Select(g => new { CommentId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CommentId, x => x.Count);

            var items = rows.Select(r => new Employee360CommentItemDto
            {
                Id = r.Id,
                Date = r.CreatedDate,
                CommentText = r.CommentText,
                TaskId = r.TaskId,
                TaskTitle = r.TaskTitle,
                AttachmentCount = attachmentCounts.TryGetValue(r.Id, out var count) ? count : 0
            }).ToList();

            return ApiResponse<PagedResponse<Employee360CommentItemDto>>.Ok(
                new PagedResponse<Employee360CommentItemDto>(items, total, pageIndex, pageSize));
        }

        private async Task<Dictionary<int, int?>> ResolveEmailTaskIdsAsync(List<EmailQueue> rows)
        {
            var result = rows.ToDictionary(r => r.Id, r => (int?)null);

            void MapDirect(ReferenceType type)
            {
                foreach (var row in rows.Where(r => r.ReferenceType == type && r.ReferenceId > 0))
                    result[row.Id] = row.ReferenceId;
            }

            MapDirect(ReferenceType.Task);
            MapDirect(ReferenceType.TaskDueTodayReminder);

            var commentIds = rows.Where(r => r.ReferenceType == ReferenceType.TaskComment).Select(r => r.ReferenceId).Distinct().ToList();
            if (commentIds.Count > 0)
            {
                var map = await _commentRepo.GetAll(c => commentIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.TaskId }).ToDictionaryAsync(c => c.Id, c => (int?)c.TaskId);
                foreach (var row in rows.Where(r => r.ReferenceType == ReferenceType.TaskComment))
                    if (map.TryGetValue(row.ReferenceId, out var tid)) result[row.Id] = tid;
            }

            var percentIds = rows.Where(r => r.ReferenceType == ReferenceType.TaskAchieve).Select(r => r.ReferenceId).Distinct().ToList();
            if (percentIds.Count > 0)
            {
                var map = await _percentageRepo.GetAll(p => percentIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.TaskId }).ToDictionaryAsync(p => p.Id, p => (int?)p.TaskId);
                foreach (var row in rows.Where(r => r.ReferenceType == ReferenceType.TaskAchieve))
                    if (map.TryGetValue(row.ReferenceId, out var tid)) result[row.Id] = tid;
            }

            var extIds = rows.Where(r =>
                    r.ReferenceType is ReferenceType.TaskExtensionRequest or ReferenceType.TaskExtensionRequestApproved)
                .Select(r => r.ReferenceId).Distinct().ToList();
            if (extIds.Count > 0)
            {
                var map = await _extensionRepo.GetAll(e => extIds.Contains(e.Id))
                    .Select(e => new { e.Id, e.TaskId }).ToDictionaryAsync(e => e.Id, e => (int?)e.TaskId);
                foreach (var row in rows.Where(r =>
                             r.ReferenceType is ReferenceType.TaskExtensionRequest or ReferenceType.TaskExtensionRequestApproved))
                    if (map.TryGetValue(row.ReferenceId, out var tid)) result[row.Id] = tid;
            }

            var closeIds = rows.Where(r =>
                    r.ReferenceType is ReferenceType.TaskCloseRequest or ReferenceType.TaskCloseRequestApproved)
                .Select(r => r.ReferenceId).Distinct().ToList();
            if (closeIds.Count > 0)
            {
                var map = await _closeRequestRepo.GetAll(c => closeIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.TaskId }).ToDictionaryAsync(c => c.Id, c => (int?)c.TaskId);
                foreach (var row in rows.Where(r =>
                             r.ReferenceType is ReferenceType.TaskCloseRequest or ReferenceType.TaskCloseRequestApproved))
                    if (map.TryGetValue(row.ReferenceId, out var tid)) result[row.Id] = tid;
            }

            var warningIds = rows.Where(r => r.ReferenceType == ReferenceType.EmployeeWarning).Select(r => r.ReferenceId).Distinct().ToList();
            if (warningIds.Count > 0)
            {
                var map = await _warningRepo.GetAll(w => warningIds.Contains(w.Id))
                    .Select(w => new { w.Id, w.TaskId }).ToDictionaryAsync(w => w.Id, w => (int?)w.TaskId);
                foreach (var row in rows.Where(r => r.ReferenceType == ReferenceType.EmployeeWarning))
                    if (map.TryGetValue(row.ReferenceId, out var tid)) result[row.Id] = tid;
            }

            var discountIds = rows.Where(r => r.ReferenceType == ReferenceType.EmployeeDeduction).Select(r => r.ReferenceId).Distinct().ToList();
            if (discountIds.Count > 0)
            {
                var map = await _discountRepo.GetAll(d => discountIds.Contains(d.Id))
                    .Select(d => new { d.Id, d.TaskId }).ToDictionaryAsync(d => d.Id, d => d.TaskId);
                foreach (var row in rows.Where(r => r.ReferenceType == ReferenceType.EmployeeDeduction))
                    if (map.TryGetValue(row.ReferenceId, out var tid)) result[row.Id] = tid;
            }

            return result;
        }

        public async Task<ApiResponse<PagedResponse<EmployeeTimelineItemDto>>> GetTimelineAsync(
            int actorId, int employeeId, EmployeeTimelineRequest request)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
            var from = NormalizeFrom(request.From);
            var to = NormalizeTo(request.To);
            var events = new List<EmployeeTimelineItemDto>();

            var assignments = await _assignmentRepo.GetAll(a =>
                    a.EmployeeId == employeeId && !a.IsDeleted &&
                    (!from.HasValue || a.AssignedAt >= from.Value) &&
                    (!to.HasValue || a.AssignedAt <= to.Value))
                .Select(a => new { a.AssignedAt, a.TaskId, TaskTitle = a.Task.Title, a.IsClosed, ClosedAt = a.Task.ClosedAt })
                .ToListAsync();

            foreach (var a in assignments)
            {
                events.Add(new EmployeeTimelineItemDto
                {
                    Date = a.AssignedAt,
                    Type = "TaskAssigned",
                    Title = "Task assigned",
                    Description = a.TaskTitle,
                    TaskId = a.TaskId,
                    TaskTitle = a.TaskTitle
                });
                if (a.IsClosed && a.ClosedAt.HasValue)
                {
                    events.Add(new EmployeeTimelineItemDto
                    {
                        Date = a.ClosedAt.Value,
                        Type = "TaskCompleted",
                        Title = "Task completed",
                        Description = a.TaskTitle,
                        TaskId = a.TaskId,
                        TaskTitle = a.TaskTitle
                    });
                }
            }

            events.AddRange((await _commentRepo.GetAll(c =>
                    c.EmployeeId == employeeId && !c.IsDeleted &&
                    (!from.HasValue || c.CreatedDate >= from.Value) &&
                    (!to.HasValue || c.CreatedDate <= to.Value))
                .Select(c => new { c.CreatedDate, c.TaskId, TaskTitle = c.Task.Title, c.CommentText })
                .ToListAsync()).Select(c => new EmployeeTimelineItemDto
            {
                Date = c.CreatedDate,
                Type = "Comment",
                Title = "Task comment",
                Description = Truncate(c.CommentText, 200),
                TaskId = c.TaskId,
                TaskTitle = c.TaskTitle
            }));

            events.AddRange((await _warningRepo.GetAll(w =>
                    w.TaskAssignment.EmployeeId == employeeId && !w.IsDeleted &&
                    (!from.HasValue || w.IssuedAt >= from.Value) &&
                    (!to.HasValue || w.IssuedAt <= to.Value))
                .Select(w => new { w.IssuedAt, w.TaskId, TaskTitle = w.Task.Title, w.Reason, w.AutoWarning })
                .ToListAsync()).Select(w => new EmployeeTimelineItemDto
            {
                Date = w.IssuedAt,
                Type = "Warning",
                Title = w.AutoWarning ? "Auto warning" : "Warning created",
                Description = Truncate(w.Reason, 200),
                TaskId = w.TaskId,
                TaskTitle = w.TaskTitle
            }));

            events.AddRange((await _discountRepo.GetAll(d =>
                    d.EmployeeId == employeeId && !d.IsDeleted &&
                    (!from.HasValue || d.ViolationDate >= from.Value) &&
                    (!to.HasValue || d.ViolationDate <= to.Value))
                .Select(d => new { d.ViolationDate, d.TaskId, TaskTitle = d.Task != null ? d.Task.Title : null, d.Reason, d.Amount })
                .ToListAsync()).Select(d => new EmployeeTimelineItemDto
            {
                Date = d.ViolationDate,
                Type = "Discount",
                Title = $"Discount added: {d.Amount:0.##}",
                Description = Truncate(d.Reason, 200),
                TaskId = d.TaskId,
                TaskTitle = d.TaskTitle
            }));

            events.AddRange((await _percentageRepo.GetAll(p =>
                    p.EmployeeId == employeeId && !p.IsDeleted &&
                    (!from.HasValue || p.CreatedDate >= from.Value) &&
                    (!to.HasValue || p.CreatedDate <= to.Value))
                .Select(p => new { p.CreatedDate, p.TaskId, TaskTitle = p.Task.Title, p.AchievementPercent, p.AchievementReason })
                .ToListAsync()).Select(p => new EmployeeTimelineItemDto
            {
                Date = p.CreatedDate,
                Type = "Progress",
                Title = $"Task progress: {p.AchievementPercent}%",
                Description = Truncate(p.AchievementReason, 200),
                TaskId = p.TaskId,
                TaskTitle = p.TaskTitle
            }));

            var leaves = await _leaveRepo.GetAll(l =>
                    l.EmployeeId == employeeId && !l.IsDeleted &&
                    (!from.HasValue || l.CreatedDate >= from.Value) &&
                    (!to.HasValue || l.CreatedDate <= to.Value))
                .Select(l => new
                {
                    l.CreatedDate,
                    l.Status,
                    LeaveTypeName = l.LeaveType.NameAr ?? l.LeaveType.NameEn,
                    l.StartDate,
                    l.EndDate,
                    l.ApprovedAt,
                    l.RejectionReason
                })
                .ToListAsync();

            foreach (var leave in leaves)
            {
                events.Add(new EmployeeTimelineItemDto
                {
                    Date = leave.CreatedDate,
                    Type = "LeaveSubmitted",
                    Title = $"Leave submitted ({leave.LeaveTypeName})",
                    Description = $"{leave.StartDate:yyyy-MM-dd} → {leave.EndDate:yyyy-MM-dd}"
                });
                if (leave.Status == LeaveStatus.Approved && leave.ApprovedAt.HasValue)
                    events.Add(new EmployeeTimelineItemDto
                    {
                        Date = leave.ApprovedAt.Value,
                        Type = "LeaveApproved",
                        Title = $"Leave approved ({leave.LeaveTypeName})",
                        Description = $"{leave.StartDate:yyyy-MM-dd} → {leave.EndDate:yyyy-MM-dd}"
                    });
                else if (leave.Status == LeaveStatus.Rejected && leave.ApprovedAt.HasValue)
                    events.Add(new EmployeeTimelineItemDto
                    {
                        Date = leave.ApprovedAt.Value,
                        Type = "LeaveRejected",
                        Title = $"Leave rejected ({leave.LeaveTypeName})",
                        Description = Truncate(leave.RejectionReason, 200)
                    });
            }

            events.AddRange((await _emailQueueRepo.GetAll(e =>
                    e.UserId == employeeId && !e.IsDeleted && e.Status == EmailStatus.Sent &&
                    (!from.HasValue || (e.SentAt ?? e.CreatedDate) >= from.Value) &&
                    (!to.HasValue || (e.SentAt ?? e.CreatedDate) <= to.Value))
                .Select(e => new { Date = e.SentAt ?? e.CreatedDate, e.TemplateKey, e.ToEmail })
                .Take(200)
                .ToListAsync()).Select(e => new EmployeeTimelineItemDto
            {
                Date = e.Date,
                Type = "EmailSent",
                Title = "Email sent",
                Description = $"{e.TemplateKey} → {e.ToEmail}"
            }));

            events.AddRange((await _notificationRepo.GetAll(n =>
                    n.UserId == employeeId && !n.IsDeleted &&
                    (!from.HasValue || n.CreatedDate >= from.Value) &&
                    (!to.HasValue || n.CreatedDate <= to.Value))
                .Select(n => new { n.CreatedDate, n.Message, n.NotificationType, n.TaskId })
                .Take(200)
                .ToListAsync()).Select(n => new EmployeeTimelineItemDto
            {
                Date = n.CreatedDate,
                Type = "NotificationSent",
                Title = "Notification sent",
                Description = Truncate(n.Message, 200),
                TaskId = n.TaskId
            }));

            IEnumerable<EmployeeTimelineItemDto> filtered = events.OrderByDescending(e => e.Date);
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.Trim();
                filtered = filtered.Where(e =>
                    (e.Title?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.Type?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.TaskTitle?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.TaskId.HasValue && e.TaskId.Value.ToString().Contains(key, StringComparison.OrdinalIgnoreCase)));
            }

            var ordered = filtered.ToList();
            var page = ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return ApiResponse<PagedResponse<EmployeeTimelineItemDto>>.Ok(
                new PagedResponse<EmployeeTimelineItemDto>(page, ordered.Count, pageIndex, pageSize));
        }

        private static DateTime? NormalizeFrom(DateTime? from) =>
            from.HasValue ? from.Value.Date : null;

        private static DateTime? NormalizeTo(DateTime? to)
        {
            if (!to.HasValue) return null;
            // Inclusive end-of-day when only a calendar date is supplied.
            return to.Value.TimeOfDay == TimeSpan.Zero
                ? to.Value.Date.AddDays(1).AddTicks(-1)
                : to.Value;
        }

        private static (DateTime? From, DateTime? To) GetNormalizedRange(Employee360DateRangeRequest? range) =>
            (NormalizeFrom(range?.From), NormalizeTo(range?.To));

        private static PeriodDto BuildPeriod(Employee360DateRangeRequest? range)
        {
            var (from, to) = GetNormalizedRange(range);
            if (!from.HasValue && !to.HasValue)
                return new PeriodDto { Type = DashboardPeriod.Year };

            return new PeriodDto
            {
                Type = DashboardPeriod.Custom,
                StartDate = from ?? DateTime.MinValue.AddYears(1),
                EndDate = to ?? DateTime.UtcNow
            };
        }

        private static bool HasCustomRange(Employee360DateRangeRequest? range) =>
            range?.From.HasValue == true || range?.To.HasValue == true;

        /// <summary>
        /// Lightweight access bits for the 360 header (roles + manager scope).
        /// Full permissions are loaded only via <see cref="GetAccessAsync"/>.
        /// </summary>
        private async Task<(List<Employee360RoleDto> Roles, Employee360ManagerScopeDto? ManagerScope)> BuildAccessSummaryAsync(int employeeId)
        {
            var roles = await _employeeRoleRepo.GetAll(er =>
                    er.EmployeeId == employeeId && er.IsAssigned && !er.IsDeleted && er.Role != null && !er.Role.IsDeleted)
                .Select(er => new Employee360RoleDto
                {
                    RoleId = er.RoleId,
                    Name = er.Role!.Name,
                    Level = er.Role.Level
                })
                .ToListAsync();

            roles = roles.GroupBy(r => r.RoleId).Select(g => g.First()).ToList();

            var accessContext = await _accessContextProvider.GetAsync(employeeId);
            Employee360ManagerScopeDto? managerScope = null;
            if (accessContext.BranchIds.Count > 0 || accessContext.EmployeeTypeIds.Count > 0)
            {
                var branches = await _managerBranchesRepo.GetAll(x =>
                        x.ManagerId == employeeId &&
                        x.IsActive &&
                        !x.IsDeleted &&
                        x.Branch != null &&
                        !x.Branch.IsDeleted &&
                        x.Branch.IsActive)
                    .Select(x => new BranchLookupDto { Id = x.BranchId, Name = x.Branch!.Name })
                    .ToListAsync();

                managerScope = new Employee360ManagerScopeDto
                {
                    EmployeeTypeIds = accessContext.EmployeeTypeIds.ToList(),
                    Branches = branches.GroupBy(b => b.Id).Select(g => g.First()).ToList()
                };
            }

            return (roles, managerScope);
        }

        private async Task<Employee360AccessDto> BuildAccessAsync(int employeeId)
        {
            var (roles, managerScope) = await BuildAccessSummaryAsync(employeeId);
            var permissions = (await _permissionService.GetPermissionsAsync(employeeId)).OrderBy(p => p).ToList();

            var groups = permissions
                .Select(p => p.Contains('_') ? p.Split('_')[0] : "GENERAL")
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            return new Employee360AccessDto
            {
                Roles = roles,
                Permissions = permissions,
                PermissionGroups = groups,
                ManagerScope = managerScope
            };
        }

        private async Task<List<Employee360ManagerDto>> GetReportingManagersAsync(int employeeId)
        {
            var managerIds = await _orgManagerResolver.GetOperationalManagersAsync(employeeId);
            if (managerIds.Count == 0) return new List<Employee360ManagerDto>();

            return await _employeeRepo.GetAll(e => managerIds.Contains(e.Id) && !e.IsDeleted)
                .Select(e => new Employee360ManagerDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    JobName = e.Job != null ? e.Job.Title : null
                })
                .ToListAsync();
        }

        public async Task<ApiResponse<PagedResponse<Employee360KpiTaskItemDto>>> GetKpiTasksAsync(
            int actorId, int employeeId, Employee360KpiTasksRequest request)
        {
            await EnsureCanViewAsync(actorId, employeeId);

            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);
            var now = DateTime.UtcNow;
            var weekEnd = now.Date.AddDays(7);

            IQueryable<TaskAssignment> query = request.Filter switch
            {
                Employee360KpiTaskFilter.Completed => SubjectClosedAssignments(employeeId),
                Employee360KpiTaskFilter.Overdue => SubjectActiveAssignments(employeeId)
                    .Where(a => a.Task.DueDate != null && a.Task.DueDate < now),
                Employee360KpiTaskFilter.Week => SubjectActiveAssignments(employeeId)
                    .Where(a => a.Task.DueDate != null && a.Task.DueDate >= now && a.Task.DueDate <= weekEnd),
                _ => SubjectActiveAssignments(employeeId)
            };

            // Distinct by TaskId (same as KPI counts)
            var taskIdsQuery = query.Select(a => a.TaskId).Distinct();
            var total = await taskIdsQuery.CountAsync();

            var pageTaskIds = await taskIdsQuery
                .OrderByDescending(id => id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var tasks = await _taskRepo.GetAll(t => pageTaskIds.Contains(t.Id))
                .Include(t => t.AssignedBy)
                .ToListAsync();

            var byId = tasks.ToDictionary(t => t.Id);
            var items = pageTaskIds
                .Where(id => byId.ContainsKey(id))
                .Select(id =>
                {
                    var t = byId[id];
                    return new Employee360KpiTaskItemDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        StatusText = t.Status.ToString(),
                        AssignedByName = t.AssignedBy?.FullName,
                        DueDate = t.DueDate,
                        CreatedByMe = t.CreatedByEmployeeId == actorId || t.AssignedByEmployeeId == actorId
                    };
                })
                .ToList();

            return ApiResponse<PagedResponse<Employee360KpiTaskItemDto>>.Ok(
                new PagedResponse<Employee360KpiTaskItemDto>(items, total, pageIndex, pageSize));
        }

        private IQueryable<TaskAssignment> SubjectAssignments(int employeeId) =>
            _assignmentRepo.GetAll(a =>
                a.EmployeeId == employeeId &&
                !a.IsDeleted &&
                !a.Task.IsDeleted);

        private IQueryable<TaskAssignment> SubjectActiveAssignments(int employeeId) =>
            SubjectAssignments(employeeId).Where(a => a.IsActive && !a.IsClosed);

        private IQueryable<TaskAssignment> SubjectClosedAssignments(int employeeId) =>
            SubjectAssignments(employeeId).Where(a => a.IsClosed);

        private async Task<Employee360KpiDto> BuildKpisAsync(int employeeId, PeriodDto? period = null)
        {
            period ??= new PeriodDto { Type = DashboardPeriod.Year };
            var dashboard = await _dashboardService.GetDashboardAsync(employeeId, period);
            var extended = await _dashboardService.GetEmployeeKpisAsync(employeeId, period);
            var now = DateTime.UtcNow;
            var weekEnd = now.Date.AddDays(7);

            var activeQuery = SubjectActiveAssignments(employeeId);

            var activeTasks = await activeQuery.Select(a => a.TaskId).Distinct().CountAsync();
            var overdue = await activeQuery
                .Where(a => a.Task.DueDate != null && a.Task.DueDate < now)
                .Select(a => a.TaskId)
                .Distinct()
                .CountAsync();
            var dueThisWeek = await activeQuery
                .Where(a => a.Task.DueDate != null && a.Task.DueDate >= now && a.Task.DueDate <= weekEnd)
                .Select(a => a.TaskId)
                .Distinct()
                .CountAsync();
            var completedTasks = await SubjectClosedAssignments(employeeId)
                .Select(a => a.TaskId)
                .Distinct()
                .CountAsync();
            var totalTasks = await SubjectAssignments(employeeId)
                .Select(a => a.TaskId)
                .Distinct()
                .CountAsync();

            var leave = await GetLeaveAsyncInternal(employeeId);

            var kpis = new Employee360KpiDto
            {
                ActiveTasks = activeTasks,
                DueSoonTasks = dashboard.Data?.Kpis?.DueSoonTasks ?? 0,
                OpenWarnings = dashboard.Data?.Kpis?.MyWarnings ?? 0,
                TotalDiscounts = dashboard.Data?.Kpis?.MyPenalties ?? 0,
                CompletedTasks = completedTasks,
                TotalTasks = totalTasks,
                AverageCompletionHours = extended.Data?.AverageCompletionHours ?? 0,
                OnTimeRatePercent = extended.Data?.OnTimeRatePercent ?? 0,
                OverdueTasks = overdue,
                DueThisWeek = dueThisWeek,
                CurrentWorkload = activeTasks,
                LeaveBalance = leave.LeaveBalance
            };

            kpis.CompletionRate = kpis.TotalTasks == 0
                ? 0
                : Math.Round(kpis.CompletedTasks * 100.0 / kpis.TotalTasks, 1);

            // Weighted performance score: on-time + completion - penalty for overdue/warnings
            var score = (kpis.OnTimeRatePercent * 0.45) + (kpis.CompletionRate * 0.35);
            score -= Math.Min(25, kpis.OverdueTasks * 3);
            score -= Math.Min(20, kpis.OpenWarnings * 2);
            kpis.PerformanceScore = Math.Round(Math.Clamp(score, 0, 100), 1);

            return kpis;
        }

        private async Task<Employee360LeaveDto> GetLeaveAsyncInternal(int employeeId)
        {
            var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var annualCap = await _leaveTypeRepo.GetAll(t => !t.IsDeleted && t.MaxDaysPerYear != null)
                .OrderByDescending(t => t.MaxDaysPerYear)
                .Select(t => (double?)t.MaxDaysPerYear)
                .FirstOrDefaultAsync() ?? 21;

            var used = await _leaveRepo.GetAll(l =>
                    l.EmployeeId == employeeId && !l.IsDeleted &&
                    l.Status == LeaveStatus.Approved && l.StartDate >= yearStart)
                .Select(l => EF.Functions.DateDiffDay(l.StartDate, l.EndDate) + 1)
                .ToListAsync();

            var usedDays = used.Sum();
            return new Employee360LeaveDto
            {
                AllowedDays = annualCap,
                UsedDays = usedDays,
                LeaveBalance = Math.Max(0, annualCap - usedDays)
            };
        }

        private async Task<Employee360SidebarDto> BuildSidebarAsync(
            int employeeId, Employee360DateRangeRequest? range = null)
        {
            var now = DateTime.UtcNow;
            var (from, to) = GetNormalizedRange(range);

            var commentsQuery = _commentRepo.GetAll(c => c.EmployeeId == employeeId && !c.IsDeleted);
            if (from.HasValue) commentsQuery = commentsQuery.Where(c => c.CreatedDate >= from.Value);
            if (to.HasValue) commentsQuery = commentsQuery.Where(c => c.CreatedDate <= to.Value);

            var comments = await commentsQuery
                .OrderByDescending(c => c.CreatedDate)
                .Take(5)
                .Select(c => new Employee360RecentCommentDto
                {
                    TaskId = c.TaskId,
                    TaskTitle = c.Task.Title,
                    CommentText = c.CommentText,
                    Date = c.CreatedDate
                })
                .ToListAsync();

            var deadlinesQuery = _assignmentRepo.GetAll(a =>
                    a.EmployeeId == employeeId && a.IsActive && !a.IsClosed && !a.IsDeleted &&
                    a.Task.DueDate != null && a.Task.DueDate >= now);
            if (from.HasValue) deadlinesQuery = deadlinesQuery.Where(a => a.Task.DueDate >= from.Value);
            if (to.HasValue) deadlinesQuery = deadlinesQuery.Where(a => a.Task.DueDate <= to.Value);

            var deadlines = await deadlinesQuery
                .OrderBy(a => a.Task.DueDate)
                .Take(5)
                .Select(a => new Employee360DeadlineDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    DueDate = a.Task.DueDate,
                    Status = a.Task.Status.ToString(),
                    Priority = a.Task.Priority.ToString()
                })
                .ToListAsync();

            var notificationsQuery = _notificationRepo.GetAll(n => n.UserId == employeeId && !n.IsDeleted);
            if (from.HasValue) notificationsQuery = notificationsQuery.Where(n => n.CreatedDate >= from.Value);
            if (to.HasValue) notificationsQuery = notificationsQuery.Where(n => n.CreatedDate <= to.Value);

            var notifications = await notificationsQuery
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .Select(n => new Employee360NotificationItemDto
                {
                    Id = n.Id,
                    Date = n.CreatedDate,
                    Title = n.Message,
                    Type = n.NotificationType.ToString(),
                    Channel = MapChannel(n.Channel),
                    Status = n.IsRead ? "Read" : "Delivered",
                    IsRead = n.IsRead,
                    TaskId = n.TaskId,
                    Delivered = true
                })
                .ToListAsync();

            var unreadQuery = _notificationRepo.GetAll(n => n.UserId == employeeId && !n.IsDeleted && !n.IsRead);
            if (from.HasValue) unreadQuery = unreadQuery.Where(n => n.CreatedDate >= from.Value);
            if (to.HasValue) unreadQuery = unreadQuery.Where(n => n.CreatedDate <= to.Value);
            var unread = await unreadQuery.CountAsync();

            var active = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId && a.IsActive && !a.IsClosed && !a.IsDeleted);

            return new Employee360SidebarDto
            {
                RecentComments = comments,
                UpcomingDeadlines = deadlines,
                RecentNotifications = notifications,
                UnreadNotifications = unread,
                ActiveAssignments = active
            };
        }

        private async Task<Employee360OverviewChartsDto> BuildOverviewChartsAsync(
            int employeeId, Employee360DateRangeRequest? range = null)
        {
            var (from, to) = GetNormalizedRange(range);
            var statusQuery = _assignmentRepo.GetAll(a => a.EmployeeId == employeeId && !a.IsDeleted);
            if (from.HasValue) statusQuery = statusQuery.Where(a => a.AssignedAt >= from.Value);
            if (to.HasValue) statusQuery = statusQuery.Where(a => a.AssignedAt <= to.Value);

            var status = await statusQuery
                .GroupBy(a => a.Task.Status)
                .Select(g => new Employee360NamedCountDto
                {
                    Name = g.Key.ToString(),
                    Count = g.Select(x => x.TaskId).Distinct().Count()
                })
                .ToListAsync();

            DateTime monthStart;
            int monthCount;
            if (HasCustomRange(range))
            {
                var start = from ?? DateTime.UtcNow.Date.AddMonths(-5);
                var end = to ?? DateTime.UtcNow;
                monthStart = new DateTime(start.Year, start.Month, 1);
                var endMonth = new DateTime(end.Year, end.Month, 1);
                monthCount = Math.Max(1, ((endMonth.Year - monthStart.Year) * 12) + endMonth.Month - monthStart.Month + 1);
                monthCount = Math.Min(monthCount, 24);
            }
            else
            {
                var start = DateTime.UtcNow.Date.AddMonths(-5);
                monthStart = new DateTime(start.Year, start.Month, 1);
                monthCount = 6;
            }

            var closedQuery = _assignmentRepo.GetAll(a =>
                    a.EmployeeId == employeeId && a.IsClosed && !a.IsDeleted &&
                    a.Task.ClosedAt != null && a.Task.ClosedAt >= monthStart);
            if (to.HasValue)
                closedQuery = closedQuery.Where(a => a.Task.ClosedAt <= to.Value);

            var closed = await closedQuery
                .Select(a => a.Task.ClosedAt!.Value)
                .ToListAsync();

            var monthly = Enumerable.Range(0, monthCount)
                .Select(i => monthStart.AddMonths(i))
                .Select(m => new Employee360NamedCountDto
                {
                    Name = m.ToString("MMM yyyy"),
                    Count = closed.Count(c => c.Year == m.Year && c.Month == m.Month)
                })
                .ToList();

            return new Employee360OverviewChartsDto
            {
                TaskStatusBreakdown = status,
                MonthlyProductivity = monthly,
                CompletionTrend = monthly
            };
        }

        private async Task EnsureCanViewAsync(int actorId, int employeeId)
        {
            var exists = await _employeeRepo.GetAll(e => e.Id == employeeId && !e.IsDeleted).AnyAsync();
            if (!exists)
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound);

            if (!await _accessScope.CanViewEmployeeAsync(actorId, employeeId))
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);
        }

        private static bool IsTaskNotification(NotificationType type) => type is
            NotificationType.Comments or NotificationType.Penalty or NotificationType.Warning or
            NotificationType.TaskAssign or NotificationType.TaskUnassign or
            NotificationType.ExtensionRequest or NotificationType.ExtensionRequestApproved or
            NotificationType.ExtensionRequestRejected or NotificationType.CloseRequest or
            NotificationType.CloseRequestApproved or NotificationType.CloseRequestRejected or
            NotificationType.AchievementPercent;

        private static string MapEmailStatus(EmailStatus status) => status switch
        {
            EmailStatus.Pending => "Queued",
            EmailStatus.Processing => "Processing",
            EmailStatus.Sent => "Sent",
            EmailStatus.Failed => "Failed",
            _ => status.ToString()
        };

        private static string MapChannel(NotificationChannel channel) => channel switch
        {
            NotificationChannel.Web => "In-App",
            NotificationChannel.Email => "Email",
            NotificationChannel.WhatsApp => "SMS",
            _ => channel.ToString()
        };

        private static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return value.Length <= max ? value : value[..max] + "…";
        }
    }
}
