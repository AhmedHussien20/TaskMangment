using System.Linq.Expressions;
using Moq;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Tests.Helpers;

namespace TaskMangment.Tests.Security;

public class OrgManagerResolverTests
{
    private readonly Mock<IRepository<Employee>> _employeeRepo = new();
    private readonly Mock<IRepository<ManagerBranches>> _managerBranchesRepo = new();
    private readonly Mock<IRepository<EmployeeFunctionalScope>> _functionalScopeRepo = new();
    private readonly Mock<IRepository<Branch>> _branchRepo = new();
    private readonly Mock<IRepository<Area>> _areaRepo = new();
    private readonly Mock<IEmployeePermissionService> _permissions = new();

    private OrgManagerResolver CreateSut() => new(
        _employeeRepo.Object,
        _managerBranchesRepo.Object,
        _functionalScopeRepo.Object,
        _branchRepo.Object,
        _areaRepo.Object,
        _permissions.Object);

    private static void SetupRepo<T>(Mock<IRepository<T>> repo, params T[] items) where T : BaseEntity
    {
        repo.Setup(r => r.GetAll(It.IsAny<Expression<Func<T, bool>>>()))
            .Returns((Expression<Func<T, bool>>? expr) =>
            {
                IEnumerable<T> source = items;
                if (expr != null)
                    source = source.Where(expr.Compile());
                return source.AsAsyncQueryable();
            });
    }

    [Fact]
    public async Task GetOperationalManagersAsync_ReturnsBranchManager()
    {
        SetupRepo(_employeeRepo,
            new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1 },
            new Employee { Id = 20, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1 });

        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 20,
            IsDeleted = false
        });

        SetupRepo(_managerBranchesRepo);
        SetupRepo(_functionalScopeRepo);
        SetupRepo(_areaRepo);

        var recipients = await CreateSut().GetOperationalManagersAsync(10);

        Assert.Contains(20, recipients);
        Assert.DoesNotContain(10, recipients);
    }

    [Fact]
    public async Task IsOrgManagerOverAsync_WhenTargetIsBranchManager_ReturnsTrue()
    {
        SetupRepo(_employeeRepo, new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true });
        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 20,
            IsDeleted = false
        });
        SetupRepo(_managerBranchesRepo);
        SetupRepo(_areaRepo);

        Assert.True(await CreateSut().IsOrgManagerOverAsync(10, 20));
    }

    [Fact]
    public async Task IsOrgManagerOverAsync_WhenTargetIsPeer_ReturnsFalse()
    {
        SetupRepo(_employeeRepo, new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true });
        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 99,
            IsDeleted = false
        });
        SetupRepo(_managerBranchesRepo);
        SetupRepo(_areaRepo);

        Assert.False(await CreateSut().IsOrgManagerOverAsync(10, 11));
    }

    [Fact]
    public async Task GetEscalationRecipientsAsync_IncludesReceiveOrgEscalationsHolders()
    {
        SetupRepo(_employeeRepo,
            new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1 },
            new Employee { Id = 30, CompanyId = 1, BranchId = 1, IsActive = true, EmployeeTypeId = 1 });

        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 10,
            IsDeleted = false,
            AreaId = null
        });
        SetupRepo(_managerBranchesRepo, new ManagerBranches
        {
            Id = 1,
            ManagerId = 10,
            BranchId = 5,
            IsActive = true,
            IsDeleted = false
        });
        SetupRepo(_areaRepo);
        SetupRepo(_functionalScopeRepo);

        _permissions
            .Setup(p => p.HasAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _permissions
            .Setup(p => p.HasAsync(30, PermissionCodes.ReceiveOrgEscalations))
            .ReturnsAsync(true);
        _permissions
            .Setup(p => p.HasAnyAsync(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(false);

        var recipients = await CreateSut().GetEscalationRecipientsAsync(10, NotificationEventKind.BranchManagerActivity);

        Assert.Contains(30, recipients);
    }
}
