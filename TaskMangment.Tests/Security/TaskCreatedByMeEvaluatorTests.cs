using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Services;

namespace TaskMangment.Tests.Security;

public class TaskCreatedByMeEvaluatorTests
{
    [Fact]
    public void Resolve_LiteralCreator_IsTrue()
    {
        var task = new WorkTask { CreatedByEmployeeId = 10, AssignedByEmployeeId = 10 };
        var scope = new ResolvedAccessScope { EmployeeId = 10, Kind = AccessScopeKind.OwnBranch };

        Assert.True(TaskCreatedByMeEvaluator.Resolve(10, task, new HashSet<int>(), scope, isActiveAssignee: false));
    }

    [Fact]
    public void Resolve_OwnBranch_SameBranchBoss_IsFalse()
    {
        // Eyad (377) + VIEW_SCOPED_TASKS → OwnBranch; Rose (279) same branch but higher — not creator-side.
        var task = new WorkTask { CreatedByEmployeeId = 279, AssignedByEmployeeId = 279 };
        var scope = new ResolvedAccessScope { EmployeeId = 377, Kind = AccessScopeKind.OwnBranch, OwnBranchId = 19 };
        var sameBranchIds = new HashSet<int> { 279 }; // would have matched before the fix

        Assert.False(TaskCreatedByMeEvaluator.Resolve(377, task, sameBranchIds, scope, isActiveAssignee: false));
        Assert.False(TaskCreatedByMeEvaluator.IsCreatorOrAssigner(377, task));
    }

    [Fact]
    public void Resolve_ManagerScoped_CreatorInManagedSet_IsTrue()
    {
        var task = new WorkTask { CreatedByEmployeeId = 50, AssignedByEmployeeId = 50 };
        var scope = new ResolvedAccessScope { EmployeeId = 1, Kind = AccessScopeKind.ManagerScoped };
        var managed = new HashSet<int> { 50 };

        Assert.True(TaskCreatedByMeEvaluator.Resolve(1, task, managed, scope, isActiveAssignee: false));
    }

    [Fact]
    public void Resolve_ManagerScoped_CreatorNotInManagedSet_IsFalse()
    {
        var task = new WorkTask { CreatedByEmployeeId = 99, AssignedByEmployeeId = 99 };
        var scope = new ResolvedAccessScope { EmployeeId = 1, Kind = AccessScopeKind.ManagerScoped };
        var managed = new HashSet<int> { 50 };

        Assert.False(TaskCreatedByMeEvaluator.Resolve(1, task, managed, scope, isActiveAssignee: false));
    }

    [Fact]
    public void Resolve_Assignee_NeverCreatorSideEvenIfCreatorInScope()
    {
        var task = new WorkTask { CreatedByEmployeeId = 50, AssignedByEmployeeId = 50 };
        var scope = new ResolvedAccessScope { EmployeeId = 20, Kind = AccessScopeKind.ManagerScoped };
        var managed = new HashSet<int> { 50 };

        Assert.False(TaskCreatedByMeEvaluator.Resolve(20, task, managed, scope, isActiveAssignee: true));
    }

    [Fact]
    public void AllowsScopeInferredCreatorSide_OnlyManagedOrCompany()
    {
        Assert.False(TaskCreatedByMeEvaluator.AllowsScopeInferredCreatorSide(AccessScopeKind.SelfOnly));
        Assert.False(TaskCreatedByMeEvaluator.AllowsScopeInferredCreatorSide(AccessScopeKind.OwnBranch));
        Assert.True(TaskCreatedByMeEvaluator.AllowsScopeInferredCreatorSide(AccessScopeKind.ManagerScoped));
        Assert.True(TaskCreatedByMeEvaluator.AllowsScopeInferredCreatorSide(AccessScopeKind.CompanyWide));
    }
}
