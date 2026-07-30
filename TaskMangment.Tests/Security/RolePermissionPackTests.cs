using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Seeding;

namespace TaskMangment.Tests.Security;

public class RolePermissionPackTests
{
    [Fact]
    public void GetPackForLevel_Admin_IncludesCompanyWideAndAssignRole()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.Admin);

        Assert.Equal(PermissionCatalog.All.Count, pack.Count);
        Assert.Contains(PermissionCodes.ViewCompanyTasks, pack);
        Assert.Contains(PermissionCodes.AssignRole, pack);
        Assert.Contains(PermissionCodes.ReceiveOrgEscalations, pack);
        Assert.Contains(PermissionCodes.CreateTask, pack);
        Assert.Contains(PermissionCodes.EnableEmployee, pack);
        Assert.Contains(PermissionCodes.CommentTask, pack);
        Assert.Contains(PermissionCodes.RequestTaskClose, pack);
        Assert.Contains(PermissionCodes.ApproveTaskRequest, pack);
        Assert.Contains(PermissionCodes.IssueWarning, pack);
        Assert.DoesNotContain("VIEW_ALL_TASKS", pack);
        Assert.DoesNotContain("EXTEND_DUE_DATE", pack);
        Assert.DoesNotContain("TRANSFER_TASK", pack);
    }

    [Fact]
    public void GetPackForLevel_BranchesManager_HasScopedNotCompanyWide()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.BranchesManager);

        Assert.Contains(PermissionCodes.ViewScopedTasks, pack);
        Assert.DoesNotContain(PermissionCodes.ViewCompanyTasks, pack);
        Assert.DoesNotContain(PermissionCodes.AssignRole, pack);
        Assert.Contains(PermissionCodes.IssuePenalty, pack);
    }

    [Fact]
    public void GetPackForLevel_Manager_HasViewEmployeesAndLeave()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.Manager);

        Assert.Contains(PermissionCodes.ViewEmployees, pack);
        Assert.Contains(PermissionCodes.ApproveLeave, pack);
        Assert.Contains(PermissionCodes.CreateTask, pack);
        Assert.DoesNotContain(PermissionCodes.ViewCompanyTasks, pack);
    }

    [Fact]
    public void GetPackForLevel_TeamLead_HasCreateTaskAndReviewBasics()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.TeamLead);

        Assert.Contains(PermissionCodes.ViewOwnTasks, pack);
        Assert.Contains(PermissionCodes.CreateTask, pack);
        Assert.Contains(PermissionCodes.ApproveTaskRequest, pack);
        Assert.Contains(PermissionCodes.RejectTaskRequest, pack);
        Assert.Contains(PermissionCodes.CommentTask, pack);
        Assert.DoesNotContain(PermissionCodes.ViewScopedTasks, pack);
        Assert.DoesNotContain(PermissionCodes.ViewEmployees, pack);
        Assert.DoesNotContain(PermissionCodes.IssueWarning, pack);
    }

    [Fact]
    public void GetPackForLevel_Employee_HasOwnTasksAndAssigneeActions()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.Employee);

        Assert.Contains(PermissionCodes.ViewOwnTasks, pack);
        Assert.Contains(PermissionCodes.CommentTask, pack);
        Assert.Contains(PermissionCodes.RequestTaskClose, pack);
        Assert.Contains(PermissionCodes.RequestDueDateExtension, pack);
        Assert.DoesNotContain(PermissionCodes.IssuePenalty, pack);
        Assert.DoesNotContain(PermissionCodes.CreateTask, pack);
    }

    [Fact]
    public void GetPackForLevel_Manager_IncludesManagerTaskActions()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.Manager);

        Assert.Contains(PermissionCodes.IssueWarning, pack);
        Assert.Contains(PermissionCodes.IssuePenalty, pack);
        Assert.Contains(PermissionCodes.ArchiveTask, pack);
    }

    [Fact]
    public void PermissionCatalog_Codes_AreDistinctAndNonEmpty()
    {
        Assert.NotEmpty(PermissionCatalog.Codes);
        Assert.Equal(PermissionCatalog.All.Count, PermissionCatalog.Codes.Count);
        Assert.Equal(PermissionCatalog.All.Select(e => e.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            PermissionCatalog.All.Count);
    }

    [Fact]
    public void PermissionCatalog_ContainsCleanTaskCodes()
    {
        Assert.Contains(PermissionCodes.CommentTask, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.RequestTaskClose, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.RequestDueDateExtension, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.ApproveTaskRequest, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.RejectTaskRequest, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.IssueWarning, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.IssuePenalty, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.ViewOwnTasks, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.ViewScopedTasks, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.ViewCompanyTasks, PermissionCatalog.Codes);
        Assert.DoesNotContain("SEND_WARNING", PermissionCatalog.Codes);
        Assert.DoesNotContain("VIEW_ALL_TASKS", PermissionCatalog.Codes);
        Assert.DoesNotContain("EXTEND_DUE_DATE", PermissionCatalog.Codes);
        Assert.DoesNotContain("TRANSFER_TASK", PermissionCatalog.Codes);
        Assert.DoesNotContain("MANAGE_MANAGER_SCOPE", PermissionCatalog.Codes);
    }
}
