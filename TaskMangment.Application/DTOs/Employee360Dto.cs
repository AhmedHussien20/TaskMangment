using System;
using System.Collections.Generic;

namespace TaskMangment.Application.DTOs
{
    public class Employee360Dto
    {
        public Employee360ProfileDto Profile { get; set; } = null!;
        public List<Employee360RoleDto> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<Employee360ManagerDto> ReportingManagers { get; set; } = new();
        public Employee360ManagerDto? DirectManager { get; set; }
        public Employee360ManagerScopeDto? ManagerScope { get; set; }
        public Employee360KpiDto Kpis { get; set; } = new();
        public Employee360SidebarDto Sidebar { get; set; } = new();
        public Employee360OverviewChartsDto OverviewCharts { get; set; } = new();
        public bool CanCreateTask { get; set; }
        public bool CanEditEmployee { get; set; }
    }

    public class Employee360ProfileDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? BranchName { get; set; }
        public string? JobName { get; set; }
        public string? DepartmentName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeTypeId { get; set; }
        public string? EmployeeTypeName { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? HireDate { get; set; }
        public string? Qualification { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public string? IdentityNumber { get; set; }
    }

    public class Employee360RoleDto
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
    }

    public class Employee360ManagerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? JobName { get; set; }
    }

    public class Employee360ManagerScopeDto
    {
        public List<int> EmployeeTypeIds { get; set; } = new();
        public List<BranchLookupDto> Branches { get; set; } = new();
    }

    public class Employee360KpiDto
    {
        public int ActiveTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int DueThisWeek { get; set; }
        public int DueSoonTasks { get; set; }
        public double CompletionRate { get; set; }
        public int OpenWarnings { get; set; }
        public decimal TotalDiscounts { get; set; }
        public double LeaveBalance { get; set; }
        public int CurrentWorkload { get; set; }
        public double PerformanceScore { get; set; }
        public double AverageCompletionHours { get; set; }
        public double OnTimeRatePercent { get; set; }
        public int TotalTasks { get; set; }
    }

    public class Employee360SidebarDto
    {
        public List<Employee360RecentCommentDto> RecentComments { get; set; } = new();
        public List<Employee360DeadlineDto> UpcomingDeadlines { get; set; } = new();
        public List<Employee360NotificationItemDto> RecentNotifications { get; set; } = new();
        public int UnreadNotifications { get; set; }
        public int ActiveAssignments { get; set; }
    }

    public class Employee360RecentCommentDto
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string? CommentText { get; set; }
        public DateTime Date { get; set; }
    }

    public class Employee360DeadlineDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }

    public class Employee360OverviewChartsDto
    {
        public List<Employee360NamedCountDto> TaskStatusBreakdown { get; set; } = new();
        public List<Employee360NamedCountDto> MonthlyProductivity { get; set; } = new();
        public List<Employee360NamedCountDto> CompletionTrend { get; set; } = new();
    }

    public class Employee360NamedCountDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class Employee360AccessDto
    {
        public List<Employee360RoleDto> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public Employee360ManagerScopeDto? ManagerScope { get; set; }
        public List<string> PermissionGroups { get; set; } = new();
    }

    public class Employee360PerformanceDto
    {
        public Employee360KpiDto Kpis { get; set; } = new();
        public List<WarningDto> Warnings { get; set; } = new();
        public List<Employee360DeadlineDto> LateTasks { get; set; } = new();
        public List<Employee360NamedCountDto> MonthlyTrend { get; set; } = new();
    }

    public class Employee360DiscountsDto
    {
        public decimal TotalAmount { get; set; }
        public List<DeductionDto> Discounts { get; set; } = new();
    }

    public class Employee360LeaveDto
    {
        public double LeaveBalance { get; set; }
        public double UsedDays { get; set; }
        public double AllowedDays { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public List<Employee360LeaveItemDto> History { get; set; } = new();
    }

    public class Employee360LeaveItemDto
    {
        public int Id { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Employee360EmailItemDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string EmailType { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
        public bool Opened { get; set; }
        public bool Clicked { get; set; }
        public int Retries { get; set; }
        public string? Provider { get; set; }
        public string? MessageId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? ClickTime { get; set; }
        public string? FailureReason { get; set; }
        public string? SmtpResponse { get; set; }
        public string? ProviderResponse { get; set; }
        public int? TaskId { get; set; }
        public string? TaskTitle { get; set; }
    }

    public class Employee360NotificationItemDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public string Channel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadTime { get; set; }
        public bool Delivered { get; set; }
        public DateTime? DeliveredTime { get; set; }
        public string? CreatedBy { get; set; }
        public int? TaskId { get; set; }
        public string? TaskTitle { get; set; }
    }

    public class EmployeeTimelineItemDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public Dictionary<string, string>? Meta { get; set; }
    }

    public class Employee360DateRangeRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class EmployeeTimelineRequest : Employee360DateRangeRequest
    {
        public string? SearchKey { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class Employee360PagedRequest : Employee360DateRangeRequest
    {
        public string? SearchKey { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Channel { get; set; }
        public bool? UnreadOnly { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Task list filter matching Employee 360 KPI card definitions.
    /// </summary>
    public enum Employee360KpiTaskFilter
    {
        Active = 1,
        Overdue = 2,
        Week = 3,
        Completed = 4
    }

    public class Employee360KpiTasksRequest
    {
        public Employee360KpiTaskFilter Filter { get; set; } = Employee360KpiTaskFilter.Active;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Employee360KpiTaskItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string? AssignedByName { get; set; }
        public DateTime? DueDate { get; set; }
        public bool CreatedByMe { get; set; }
    }
}
