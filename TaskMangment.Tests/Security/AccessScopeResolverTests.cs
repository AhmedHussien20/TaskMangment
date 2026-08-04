using System.Linq.Expressions;
using Moq;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Tests.Helpers;

namespace TaskMangment.Tests.Security;

public class AccessScopeResolverTests
{
    private readonly Mock<IRepository<Employee>> _employeeRepo = new();
    private readonly Mock<IRepository<Branch>> _branchRepo = new();
    private readonly Mock<IRepository<Area>> _areaRepo = new();
    private readonly Mock<IUserAccessContextProvider> _accessProvider = new();
    private readonly Mock<IEmployeePermissionService> _permissions = new();
    private readonly Mock<IOrgManagerResolver> _orgManagers = new();

    private AccessScopeResolver CreateSut() =>
        new(_employeeRepo.Object, _branchRepo.Object, _areaRepo.Object,
            _accessProvider.Object, _permissions.Object, _orgManagers.Object);

    public AccessScopeResolverTests()
    {
        // Default empty org trees so ResolveAsync ManagerScoped/OwnBranch can compute excludes.
        _branchRepo
            .Setup(r => r.GetAll(It.IsAny<Expression<Func<Branch, bool>>>()))
            .Returns((Expression<Func<Branch, bool>>? expr) =>
            {
                IEnumerable<Branch> source = Array.Empty<Branch>();
                if (expr != null)
                    source = source.Where(expr.Compile());
                return source.AsAsyncQueryable();
            });
        _areaRepo
            .Setup(r => r.GetAll(It.IsAny<Expression<Func<Area, bool>>>()))
            .Returns((Expression<Func<Area, bool>>? expr) =>
            {
                IEnumerable<Area> source = Array.Empty<Area>();
                if (expr != null)
                    source = source.Where(expr.Compile());
                return source.AsAsyncQueryable();
            });
    }

    private void SetupEmployees(params Employee[] employees)
    {
        _employeeRepo
            .Setup(r => r.GetAll(It.IsAny<Expression<Func<Employee, bool>>>()))
            .Returns((Expression<Func<Employee, bool>>? expr) =>
            {
                IEnumerable<Employee> source = employees;
                if (expr != null)
                    source = source.Where(expr.Compile());
                return source.AsAsyncQueryable();
            });
    }

    private void SetupHasActiveRole(int employeeId, bool hasRole = true)
    {
        _permissions.Setup(p => p.HasActiveRoleAsync(employeeId)).ReturnsAsync(hasRole);
    }

    [Fact]
    public async Task ResolveAsync_WithViewCompanyTasks_IsCompanyWide()
    {
        SetupEmployees(new Employee { Id = 1, CompanyId = 10, BranchId = 5, IsActive = true });
        SetupHasActiveRole(1);
        _accessProvider.Setup(a => a.GetAsync(1))
            .ReturnsAsync(new UserAccessContext { EmployeeId = 1 });
        _permissions.Setup(p => p.GetPermissionsAsync(1))
            .ReturnsAsync(new HashSet<string> { PermissionCodes.ViewCompanyTasks });

        var scope = await CreateSut().ResolveAsync(1);

        Assert.Equal(AccessScopeKind.CompanyWide, scope.Kind);
        Assert.Equal(10, scope.CompanyId);
        Assert.Empty(scope.ViewExcludeEmployeeIds);
    }

    [Fact]
    public async Task ResolveAsync_WithManagerBranches_IsManagerScoped()
    {
        SetupEmployees(new Employee { Id = 2, CompanyId = 10, BranchId = 5, IsActive = true });
        SetupHasActiveRole(2);
        _accessProvider.Setup(a => a.GetAsync(2))
            .ReturnsAsync(new UserAccessContext
            {
                EmployeeId = 2,
                BranchIds = [5, 6]
            });
        _permissions.Setup(p => p.GetPermissionsAsync(2))
            .ReturnsAsync(new HashSet<string> { PermissionCodes.ViewScopedTasks });

        var scope = await CreateSut().ResolveAsync(2);

        Assert.Equal(AccessScopeKind.ManagerScoped, scope.Kind);
        Assert.Equal(new[] { 5, 6 }, scope.BranchIds);
    }

    [Fact]
    public async Task ResolveAsync_WithCreateTaskOnly_IsOwnBranch()
    {
        SetupEmployees(new Employee { Id = 3, CompanyId = 10, BranchId = 7, IsActive = true });
        SetupHasActiveRole(3);
        _accessProvider.Setup(a => a.GetAsync(3))
            .ReturnsAsync(new UserAccessContext { EmployeeId = 3 });
        _permissions.Setup(p => p.GetPermissionsAsync(3))
            .ReturnsAsync(new HashSet<string> { PermissionCodes.CreateTask });

        var scope = await CreateSut().ResolveAsync(3);

        Assert.Equal(AccessScopeKind.OwnBranch, scope.Kind);
        Assert.Equal(7, scope.OwnBranchId);
    }

    [Fact]
    public async Task ResolveAsync_WithOnlyViewOwnTasks_IsSelfOnly()
    {
        SetupEmployees(new Employee { Id = 4, CompanyId = 10, BranchId = 7, IsActive = true });
        SetupHasActiveRole(4);
        _accessProvider.Setup(a => a.GetAsync(4))
            .ReturnsAsync(new UserAccessContext { EmployeeId = 4 });
        _permissions.Setup(p => p.GetPermissionsAsync(4))
            .ReturnsAsync(new HashSet<string> { PermissionCodes.ViewOwnTasks });

        var scope = await CreateSut().ResolveAsync(4);

        Assert.Equal(AccessScopeKind.SelfOnly, scope.Kind);
    }

    [Fact]
    public async Task ResolveAsync_NoRole_IgnoresFunctionalScope_IsSelfOnly()
    {
        SetupEmployees(new Employee { Id = 130, CompanyId = 10, BranchId = 3, IsActive = true });
        SetupHasActiveRole(130, hasRole: false);
        _accessProvider.Setup(a => a.GetAsync(130))
            .ReturnsAsync(new UserAccessContext
            {
                EmployeeId = 130,
                EmployeeTypeIds = [1],
                SeesAllTypesInBranchScope = true
            });

        var scope = await CreateSut().ResolveAsync(130);

        Assert.Equal(AccessScopeKind.SelfOnly, scope.Kind);
        Assert.Empty(scope.BranchIds);
        Assert.Empty(scope.EmployeeTypeIds);
        Assert.False(scope.SeesAllTypesInBranchScope);
        _accessProvider.Verify(a => a.GetAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void FilterEmployees_OwnBranch_ReturnsSameBranchOnly()
    {
        var employees = new List<Employee>
        {
            new() { Id = 1, CompanyId = 10, BranchId = 7, IsActive = true },
            new() { Id = 2, CompanyId = 10, BranchId = 8, IsActive = true },
            new() { Id = 3, CompanyId = 10, BranchId = 7, IsActive = true },
            new() { Id = 4, CompanyId = 99, BranchId = 7, IsActive = true }
        }.AsQueryable();

        var scope = new ResolvedAccessScope
        {
            EmployeeId = 1,
            CompanyId = 10,
            OwnBranchId = 7,
            Kind = AccessScopeKind.OwnBranch
        };

        var result = CreateSut().FilterEmployees(employees, scope).Select(e => e.Id).ToList();

        Assert.Equal(new[] { 1, 3 }, result);
    }

    [Fact]
    public void FilterEmployees_View_ExcludesConfiguredSuperiors()
    {
        var employees = new List<Employee>
        {
            new() { Id = 104, CompanyId = 10, BranchId = 4, IsActive = true },
            new() { Id = 103, CompanyId = 10, BranchId = 4, IsActive = true }, // area manager
            new() { Id = 6, CompanyId = 10, BranchId = 4, IsActive = true },   // company-wide
            new() { Id = 200, CompanyId = 10, BranchId = 4, IsActive = true }  // normal staff
        }.AsQueryable();

        var scope = new ResolvedAccessScope
        {
            EmployeeId = 104,
            CompanyId = 10,
            OwnBranchId = 4,
            Kind = AccessScopeKind.ManagerScoped,
            BranchIds = [4],
            ViewExcludeEmployeeIds = [103, 6]
        };

        var result = CreateSut().FilterEmployees(employees, scope).Select(e => e.Id).OrderBy(x => x).ToList();

        Assert.Equal(new[] { 104, 200 }, result);
    }

    [Fact]
    public void FilterEmployees_Assign_DoesNotApplyViewExcludes()
    {
        var employees = new List<Employee>
        {
            new() { Id = 104, CompanyId = 10, BranchId = 4, IsActive = true },
            new() { Id = 103, CompanyId = 10, BranchId = 4, IsActive = true }
        }.AsQueryable();

        var scope = new ResolvedAccessScope
        {
            EmployeeId = 104,
            CompanyId = 10,
            Kind = AccessScopeKind.ManagerScoped,
            BranchIds = [4],
            ViewExcludeEmployeeIds = [103]
        };

        var result = CreateSut()
            .FilterEmployees(employees, scope, AccessIntent.Assign)
            .Select(e => e.Id)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(new[] { 103, 104 }, result);
    }

    [Fact]
    public void FilterEmployees_ManagerScoped_UnionsBranchesOrTypes()
    {
        var employees = new List<Employee>
        {
            new() { Id = 1, CompanyId = 10, BranchId = 5, EmployeeTypeId = 1, IsActive = true }, // in branch
            new() { Id = 2, CompanyId = 10, BranchId = 9, EmployeeTypeId = 3, IsActive = true }, // matching type
            new() { Id = 3, CompanyId = 10, BranchId = 9, EmployeeTypeId = 1, IsActive = true }, // neither
            new() { Id = 4, CompanyId = 99, BranchId = 5, EmployeeTypeId = 3, IsActive = true }  // other company
        }.AsQueryable();

        var scope = new ResolvedAccessScope
        {
            EmployeeId = 100,
            CompanyId = 10,
            Kind = AccessScopeKind.ManagerScoped,
            BranchIds = [5],
            EmployeeTypeIds = [3],
            SeesAllTypesInBranchScope = false
        };

        var result = CreateSut().FilterEmployees(employees, scope).Select(e => e.Id).OrderBy(x => x).ToList();

        Assert.Equal(new[] { 1, 2 }, result);
    }

    [Fact]
    public void FilterEmployees_ManagerScoped_RestrictedType_IsTypeAndActorBranch()
    {
        var employees = new List<Employee>
        {
            new() { Id = 1, CompanyId = 10, BranchId = 5, EmployeeTypeId = 3, IsActive = true }, // accounting in home
            new() { Id = 2, CompanyId = 10, BranchId = 9, EmployeeTypeId = 3, IsActive = true }, // accounting other branch
            new() { Id = 3, CompanyId = 10, BranchId = 5, EmployeeTypeId = 1, IsActive = true }, // other type in home
        }.AsQueryable();

        var scope = new ResolvedAccessScope
        {
            EmployeeId = 100,
            CompanyId = 10,
            OwnBranchId = 5,
            Kind = AccessScopeKind.ManagerScoped,
            BranchIds = [],
            EmployeeTypeIds = [3],
            BranchRestrictedEmployeeTypeIds = [3],
            SeesAllTypesInBranchScope = false
        };

        var result = CreateSut().FilterEmployees(employees, scope).Select(e => e.Id).OrderBy(x => x).ToList();

        Assert.Equal(new[] { 1 }, result);
    }

    [Fact]
    public void FilterEmployees_SelfOnly_ReturnsActorOnly()
    {
        var employees = new List<Employee>
        {
            new() { Id = 1, CompanyId = 10, BranchId = 7, IsActive = true },
            new() { Id = 2, CompanyId = 10, BranchId = 7, IsActive = true }
        }.AsQueryable();

        var scope = new ResolvedAccessScope
        {
            EmployeeId = 1,
            CompanyId = 10,
            Kind = AccessScopeKind.SelfOnly
        };

        var result = CreateSut().FilterEmployees(employees, scope).Select(e => e.Id).ToList();

        Assert.Equal(new[] { 1 }, result);
    }

    [Fact]
    public async Task CanAssignAsync_WithoutCreateTask_ReturnsFalse()
    {
        SetupEmployees(
            new Employee { Id = 1, CompanyId = 10, BranchId = 7, IsActive = true },
            new Employee { Id = 2, CompanyId = 10, BranchId = 7, IsActive = true });

        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.CreateTask)).ReturnsAsync(false);

        Assert.False(await CreateSut().CanAssignAsync(1, 2));
    }

    [Fact]
    public async Task CanAssignAsync_OrgManagerWithoutAssignToManagers_ReturnsFalse()
    {
        SetupEmployees(
            new Employee { Id = 1, CompanyId = 10, BranchId = 7, IsActive = true },
            new Employee { Id = 2, CompanyId = 10, BranchId = 7, IsActive = true });

        _accessProvider.Setup(a => a.GetAsync(1))
            .ReturnsAsync(new UserAccessContext { EmployeeId = 1 });
        _permissions.Setup(p => p.HasActiveRoleAsync(1)).ReturnsAsync(true);
        _permissions.Setup(p => p.GetPermissionsAsync(1))
            .ReturnsAsync(new HashSet<string> { PermissionCodes.CreateTask });
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.CreateTask)).ReturnsAsync(true);
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.AssignOutsideScope)).ReturnsAsync(false);
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.AssignToManagers)).ReturnsAsync(false);
        _orgManagers.Setup(o => o.IsOrgManagerOverAsync(1, 2)).ReturnsAsync(true);

        Assert.False(await CreateSut().CanAssignAsync(1, 2));
    }

    [Fact]
    public async Task CanAssignAsync_OrgManagerWithAssignToManagers_ReturnsTrue()
    {
        SetupEmployees(
            new Employee { Id = 1, CompanyId = 10, BranchId = 7, IsActive = true },
            new Employee { Id = 2, CompanyId = 10, BranchId = 7, IsActive = true });

        _accessProvider.Setup(a => a.GetAsync(1))
            .ReturnsAsync(new UserAccessContext { EmployeeId = 1 });
        _permissions.Setup(p => p.HasActiveRoleAsync(1)).ReturnsAsync(true);
        _permissions.Setup(p => p.GetPermissionsAsync(1))
            .ReturnsAsync(new HashSet<string> { PermissionCodes.CreateTask });
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.CreateTask)).ReturnsAsync(true);
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.AssignOutsideScope)).ReturnsAsync(false);
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.AssignToManagers)).ReturnsAsync(true);
        _orgManagers.Setup(o => o.IsOrgManagerOverAsync(1, 2)).ReturnsAsync(true);

        Assert.True(await CreateSut().CanAssignAsync(1, 2));
    }

    [Fact]
    public async Task CanAssignAsync_SameBranchPeer_ReturnsTrue()
    {
        SetupEmployees(
            new Employee { Id = 1, CompanyId = 10, BranchId = 7, IsActive = true },
            new Employee { Id = 2, CompanyId = 10, BranchId = 7, IsActive = true });

        _accessProvider.Setup(a => a.GetAsync(1))
            .ReturnsAsync(new UserAccessContext { EmployeeId = 1 });
        _permissions.Setup(p => p.HasActiveRoleAsync(1)).ReturnsAsync(true);
        _permissions.Setup(p => p.GetPermissionsAsync(1))
            .ReturnsAsync(new HashSet<string> { PermissionCodes.CreateTask });
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.CreateTask)).ReturnsAsync(true);
        _permissions.Setup(p => p.HasAsync(1, PermissionCodes.AssignOutsideScope)).ReturnsAsync(false);
        _orgManagers.Setup(o => o.IsOrgManagerOverAsync(1, 2)).ReturnsAsync(false);

        Assert.True(await CreateSut().CanAssignAsync(1, 2));
    }
}
