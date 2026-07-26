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
        Assert.Contains(PermissionCodes.ViewAllTasks, pack);
        Assert.Contains(PermissionCodes.AssignRole, pack);
        Assert.Contains(PermissionCodes.ReceiveOrgEscalations, pack);
        Assert.Contains(PermissionCodes.CreateTask, pack);
        Assert.Contains(PermissionCodes.EnableEmployee, pack);
        Assert.Contains(PermissionCodes.CommentTask, pack);
    }

    [Fact]
    public void GetPackForLevel_BranchesManager_HasScopedNotCompanyWide()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.BranchesManager);

        Assert.Contains(PermissionCodes.ViewScopedTasks, pack);
        Assert.Contains(PermissionCodes.ManageManagerScope, pack);
        Assert.DoesNotContain(PermissionCodes.ViewCompanyTasks, pack);
        Assert.DoesNotContain(PermissionCodes.AssignRole, pack);
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
        Assert.Contains(PermissionCodes.ApproveCloseExtend, pack);
        Assert.Contains(PermissionCodes.CommentTask, pack);
        Assert.DoesNotContain(PermissionCodes.ViewScopedTasks, pack);
        Assert.DoesNotContain(PermissionCodes.ViewEmployees, pack);
        Assert.DoesNotContain(PermissionCodes.SendWarning, pack);
    }

    [Fact]
    public void GetPackForLevel_Employee_HasOwnTasksAndAssigneeActions()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.Employee);

        Assert.Contains(PermissionCodes.ViewOwnTasks, pack);
        Assert.Contains(PermissionCodes.CommentTask, pack);
        Assert.Contains(PermissionCodes.CloseTaskEmployee, pack);
        Assert.Contains(PermissionCodes.SubmitDueDate, pack);
        Assert.DoesNotContain(PermissionCodes.SendPenalty, pack);
        Assert.DoesNotContain(PermissionCodes.CreateTask, pack);
    }

    [Fact]
    public void GetPackForLevel_Manager_IncludesManagerTaskActions()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.Manager);

        Assert.Contains(PermissionCodes.SendWarning, pack);
        Assert.Contains(PermissionCodes.SendPenalty, pack);
        Assert.Contains(PermissionCodes.ArchiveTask, pack);
    }

    [Fact]
    public void NewCatalogCodes_AreDistinctAndNonEmpty()
    {
        Assert.NotEmpty(PermissionCodes.NewCatalogCodes);
        Assert.Equal(PermissionCodes.NewCatalogCodes.Length, PermissionCodes.NewCatalogCodes.Distinct().Count());
    }

    [Fact]
    public void PermissionCatalog_ContainsAllCanonicalCodesOnce()
    {
        Assert.Equal(52, PermissionCatalog.All.Count);
        Assert.Equal(PermissionCatalog.All.Count, PermissionCatalog.Codes.Count);
        Assert.Contains(PermissionCodes.CommentTask, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.SendWarning, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.SendPenalty, PermissionCatalog.Codes);
        Assert.Contains(PermissionCodes.ApproveLeave, PermissionCatalog.Codes);
    }
}
