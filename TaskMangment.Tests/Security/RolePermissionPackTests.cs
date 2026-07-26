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

        Assert.Contains(PermissionCodes.ViewCompanyTasks, pack);
        Assert.Contains(PermissionCodes.ViewAllTasks, pack);
        Assert.Contains(PermissionCodes.AssignRole, pack);
        Assert.Contains(PermissionCodes.ReceiveOrgEscalations, pack);
        Assert.Contains(PermissionCodes.CreateTask, pack);
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
    public void GetPackForLevel_TeamLead_HasCreateTaskOnlyBasics()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.TeamLead);

        Assert.Contains(PermissionCodes.ViewOwnTasks, pack);
        Assert.Contains(PermissionCodes.CreateTask, pack);
        Assert.DoesNotContain(PermissionCodes.ViewScopedTasks, pack);
        Assert.DoesNotContain(PermissionCodes.ViewEmployees, pack);
    }

    [Fact]
    public void GetPackForLevel_Employee_OnlyViewOwnTasks()
    {
        var pack = RolePermissionPackMigrator.GetPackForLevel((int)RoleLevelEnum.Employee);

        Assert.Equal(new[] { PermissionCodes.ViewOwnTasks }, pack);
    }

    [Fact]
    public void NewCatalogCodes_AreDistinctAndNonEmpty()
    {
        Assert.NotEmpty(PermissionCodes.NewCatalogCodes);
        Assert.Equal(PermissionCodes.NewCatalogCodes.Length, PermissionCodes.NewCatalogCodes.Distinct().Count());
    }
}
