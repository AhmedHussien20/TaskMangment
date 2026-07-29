using System.Linq.Expressions;
using Moq;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Tests.Helpers;

namespace TaskMangment.Tests.Security;

public class OrgManagerResolverTests
{
    private readonly Mock<IRepository<Employee>> _employeeRepo = new();
    private readonly Mock<IRepository<EmployeeRole>> _employeeRoleRepo = new();
    private readonly Mock<IRepository<RoleNotificationSource>> _notifySourceRepo = new();
    private readonly Mock<IRepository<Branch>> _branchRepo = new();
    private readonly Mock<IRepository<Area>> _areaRepo = new();

    private OrgManagerResolver CreateSut() => new(
        _employeeRepo.Object,
        _employeeRoleRepo.Object,
        _notifySourceRepo.Object,
        _branchRepo.Object,
        _areaRepo.Object);

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

    private static EmployeeType OpsType() => new()
    {
        Id = 1,
        Code = EmployeeTypeCodes.Operations,
        NameEn = "Operations",
        SeesAllTypesInBranchScope = true
    };

    private static EmployeeType HrType() => new()
    {
        Id = 3,
        Code = EmployeeTypeCodes.HR,
        NameEn = "HR",
        SeesAllTypesInBranchScope = false
    };

    [Fact]
    public async Task BranchScope_NotifiesBranchManagerWithListeningRole()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var branchMgrRole = new Role { Id = 4, Name = "Branch Manager", NotificationScope = NotificationScope.Branch };

        var subject = new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var manager = new Employee { Id = 20, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, subject, manager);
        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 20,
            IsActive = true,
            IsDeleted = false
        });
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 10, RoleId = 7, IsAssigned = true, Employee = subject, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 20, RoleId = 4, IsAssigned = true, Employee = manager, Role = branchMgrRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 4,
            SourceRoleId = 7,
            Role = branchMgrRole,
            IsDeleted = false
        });

        var recipients = await CreateSut().GetOperationalManagersAsync(10);

        Assert.Contains(20, recipients);
        Assert.DoesNotContain(10, recipients);
    }

    [Fact]
    public async Task BranchScope_NotifiesAllListeningRoleHoldersInSameBranch_EvenIfNotManagerID()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var trainingSupervisorRole = new Role { Id = 9, Name = "Training Supervisor", NotificationScope = NotificationScope.Branch };

        var subject = new Employee { Id = 294, CompanyId = 1, BranchId = 9, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var supervisor = new Employee { Id = 293, CompanyId = 1, BranchId = 9, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var otherBranchSupervisor = new Employee { Id = 999, CompanyId = 1, BranchId = 3, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        // Official branch manager — different person, no listening role
        var branchManager = new Employee { Id = 381, CompanyId = 1, BranchId = 9, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, subject, supervisor, otherBranchSupervisor, branchManager);
        SetupRepo(_branchRepo, new Branch
        {
            Id = 9,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 381,
            IsActive = true,
            IsDeleted = false
        });
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 294, RoleId = 7, IsAssigned = true, Employee = subject, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 293, RoleId = 9, IsAssigned = true, Employee = supervisor, Role = trainingSupervisorRole },
            new EmployeeRole { Id = 3, EmployeeId = 999, RoleId = 9, IsAssigned = true, Employee = otherBranchSupervisor, Role = trainingSupervisorRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 9,
            SourceRoleId = 7,
            Role = trainingSupervisorRole,
            IsDeleted = false
        });

        var recipients = await CreateSut().GetOperationalManagersAsync(294);

        Assert.Contains(293, recipients);
        Assert.DoesNotContain(999, recipients);
        Assert.DoesNotContain(381, recipients);
        Assert.DoesNotContain(294, recipients);
    }

    [Fact]
    public async Task BranchScope_AlsoNotifiesAreaManagerWithListeningRole()
    {
        var trainingSupervisorRole = new Role { Id = 9, Name = "Training Supervisor", NotificationScope = NotificationScope.None };
        var branchMgrRole = new Role { Id = 4, Name = "Branch Manager", NotificationScope = NotificationScope.Branch };

        var subject = new Employee { Id = 298, CompanyId = 1, BranchId = 20, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var areaMgrListener = new Employee { Id = 279, CompanyId = 1, BranchId = 19, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var otherAreaListener = new Employee { Id = 999, CompanyId = 1, BranchId = 1, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, subject, areaMgrListener, otherAreaListener);
        SetupRepo(_branchRepo,
            new Branch { Id = 20, CompanyId = 1, Name = "Khamis", ManagerID = 1, AreaId = 11, IsActive = true, IsDeleted = false },
            new Branch { Id = 19, CompanyId = 1, Name = "Root", ManagerID = 279, AreaId = 11, IsActive = true, IsDeleted = false });
        SetupRepo(_areaRepo,
            new Area { Id = 11, Name = "Jordan", ManagerEmployeeId = 279, IsDeleted = false },
            new Area { Id = 1, Name = "Other", ManagerEmployeeId = 999, IsDeleted = false });
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 298, RoleId = 9, IsAssigned = true, Employee = subject, Role = trainingSupervisorRole },
            new EmployeeRole { Id = 2, EmployeeId = 279, RoleId = 4, IsAssigned = true, Employee = areaMgrListener, Role = branchMgrRole },
            new EmployeeRole { Id = 3, EmployeeId = 999, RoleId = 4, IsAssigned = true, Employee = otherAreaListener, Role = branchMgrRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 4,
            SourceRoleId = 9,
            Role = branchMgrRole,
            IsDeleted = false
        });

        var recipients = await CreateSut().GetOperationalManagersAsync(298);
        Assert.Contains(279, recipients);
        Assert.DoesNotContain(999, recipients);

        var subjects = await CreateSut().GetListenableSubjectIdsAsync(279);
        Assert.Contains(298, subjects);
    }

    [Fact]
    public async Task BranchScope_OrgManager_NotNotifiedViaHome_WhenAnotherIsBranchManager()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var branchMgrRole = new Role { Id = 4, Name = "Branch Manager", NotificationScope = NotificationScope.Branch };

        // Subject lives in branch 5 (official manager = 381).
        var subject = new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        // Org manager whose HOME is also branch 5, but manages a different branch (6).
        var otherBranchMgr = new Employee { Id = 200, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var officialMgr = new Employee { Id = 381, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, subject, otherBranchMgr, officialMgr);
        SetupRepo(_branchRepo,
            new Branch { Id = 5, CompanyId = 1, Name = "HomeBranch", ManagerID = 381, IsActive = true, IsDeleted = false },
            new Branch { Id = 6, CompanyId = 1, Name = "Other", ManagerID = 200, IsActive = true, IsDeleted = false });
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 10, RoleId = 7, IsAssigned = true, Employee = subject, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 200, RoleId = 4, IsAssigned = true, Employee = otherBranchMgr, Role = branchMgrRole },
            new EmployeeRole { Id = 3, EmployeeId = 381, RoleId = 4, IsAssigned = true, Employee = officialMgr, Role = branchMgrRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 4,
            SourceRoleId = 7,
            Role = branchMgrRole,
            IsDeleted = false
        });

        var recipients = await CreateSut().GetOperationalManagersAsync(10);
        Assert.Contains(381, recipients);
        Assert.DoesNotContain(200, recipients);

        var subjectsForOther = await CreateSut().GetListenableSubjectIdsAsync(200);
        Assert.DoesNotContain(10, subjectsForOther);

        var subjectsForOfficial = await CreateSut().GetListenableSubjectIdsAsync(381);
        Assert.Contains(10, subjectsForOfficial);
    }

    [Fact]
    public async Task GetListenableSubjectIds_BranchScope_ReturnsSourceRoleEmployeesInSameBranch()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var trainingSupervisorRole = new Role { Id = 9, Name = "Training Supervisor", NotificationScope = NotificationScope.Branch };

        var subject = new Employee { Id = 294, CompanyId = 1, BranchId = 9, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var otherBranchSubject = new Employee { Id = 295, CompanyId = 1, BranchId = 3, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var supervisor = new Employee { Id = 293, CompanyId = 1, BranchId = 9, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, subject, otherBranchSubject, supervisor);
        SetupRepo(_branchRepo, new Branch
        {
            Id = 9,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 381,
            IsActive = true,
            IsDeleted = false
        },
        new Branch
        {
            Id = 3,
            CompanyId = 1,
            Name = "Other",
            ManagerID = 1,
            IsActive = true,
            IsDeleted = false
        });
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 294, RoleId = 7, IsAssigned = true, Employee = subject, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 295, RoleId = 7, IsAssigned = true, Employee = otherBranchSubject, Role = employeeRole },
            new EmployeeRole { Id = 3, EmployeeId = 293, RoleId = 9, IsAssigned = true, Employee = supervisor, Role = trainingSupervisorRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 9,
            SourceRoleId = 7,
            Role = trainingSupervisorRole,
            IsDeleted = false
        });

        var subjects = await CreateSut().GetListenableSubjectIdsAsync(293);

        Assert.Contains(294, subjects);
        Assert.DoesNotContain(295, subjects);
        Assert.DoesNotContain(293, subjects);
    }

    [Fact]
    public async Task CompanyScope_NonOperations_OnlySameType()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var hrRole = new Role { Id = 12, Name = "HR", NotificationScope = NotificationScope.Company };

        var subjectHr = new Employee { Id = 10, CompanyId = 1, BranchId = 1, IsActive = true, EmployeeTypeId = 3, EmployeeType = HrType() };
        var subjectOps = new Employee { Id = 11, CompanyId = 1, BranchId = 1, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var hrListener = new Employee { Id = 30, CompanyId = 1, BranchId = 2, IsActive = true, EmployeeTypeId = 3, EmployeeType = HrType() };

        SetupRepo(_employeeRepo, subjectHr, subjectOps, hrListener);
        SetupRepo(_branchRepo);
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 10, RoleId = 7, IsAssigned = true, Employee = subjectHr, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 11, RoleId = 7, IsAssigned = true, Employee = subjectOps, Role = employeeRole },
            new EmployeeRole { Id = 3, EmployeeId = 30, RoleId = 12, IsAssigned = true, Employee = hrListener, Role = hrRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 12,
            SourceRoleId = 7,
            Role = hrRole,
            IsDeleted = false
        });

        var fromHr = await CreateSut().GetOperationalManagersAsync(10);
        var fromOps = await CreateSut().GetOperationalManagersAsync(11);

        Assert.Contains(30, fromHr);
        Assert.DoesNotContain(30, fromOps);
    }

    [Fact]
    public async Task CompanyScope_Operations_HearsAllTypes()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var companyRole = new Role { Id = 99, Name = "Ops Lead", NotificationScope = NotificationScope.Company };

        var subjectHr = new Employee { Id = 10, CompanyId = 1, BranchId = 1, IsActive = true, EmployeeTypeId = 3, EmployeeType = HrType() };
        var opsListener = new Employee { Id = 40, CompanyId = 1, BranchId = 2, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, subjectHr, opsListener);
        SetupRepo(_branchRepo);
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 10, RoleId = 7, IsAssigned = true, Employee = subjectHr, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 40, RoleId = 99, IsAssigned = true, Employee = opsListener, Role = companyRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 99,
            SourceRoleId = 7,
            Role = companyRole,
            IsDeleted = false
        });

        var recipients = await CreateSut().GetOperationalManagersAsync(10);
        Assert.Contains(40, recipients);
    }

    [Fact]
    public async Task MultiRole_UnionsBranchAndCompanyPaths()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var branchMgrRole = new Role { Id = 4, Name = "Branch Manager", NotificationScope = NotificationScope.Branch };
        var hrRole = new Role { Id = 12, Name = "HR", NotificationScope = NotificationScope.Company };

        var subject = new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 3, EmployeeType = HrType() };
        var dual = new Employee { Id = 50, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 3, EmployeeType = HrType() };

        SetupRepo(_employeeRepo, subject, dual);
        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 50,
            IsActive = true,
            IsDeleted = false
        });
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 10, RoleId = 7, IsAssigned = true, Employee = subject, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 50, RoleId = 4, IsAssigned = true, Employee = dual, Role = branchMgrRole },
            new EmployeeRole { Id = 3, EmployeeId = 50, RoleId = 12, IsAssigned = true, Employee = dual, Role = hrRole });
        SetupRepo(_notifySourceRepo,
            new RoleNotificationSource { Id = 1, RoleId = 4, SourceRoleId = 7, Role = branchMgrRole, IsDeleted = false },
            new RoleNotificationSource { Id = 2, RoleId = 12, SourceRoleId = 7, Role = hrRole, IsDeleted = false });

        var recipients = await CreateSut().GetOperationalManagersAsync(10);
        Assert.Contains(50, recipients);
    }

    [Fact]
    public async Task AreaScope_NotifiesAreaManager_WhenSourceIsBranchManager()
    {
        var branchMgrRole = new Role { Id = 4, Name = "Branch Manager", NotificationScope = NotificationScope.Branch };
        var areaMgrRole = new Role { Id = 14, Name = "Area Manager", NotificationScope = NotificationScope.Area };

        var branchMgr = new Employee { Id = 20, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var areaMgr = new Employee { Id = 90, CompanyId = 1, BranchId = 1, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        var otherAreaMgr = new Employee { Id = 91, CompanyId = 1, BranchId = 2, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, branchMgr, areaMgr, otherAreaMgr);
        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 20,
            AreaId = 8,
            IsActive = true,
            IsDeleted = false
        });
        SetupRepo(_areaRepo,
            new Area { Id = 8, Name = "East", ManagerEmployeeId = 90, IsDeleted = false },
            new Area { Id = 9, Name = "South", ManagerEmployeeId = 91, IsDeleted = false });
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 20, RoleId = 4, IsAssigned = true, Employee = branchMgr, Role = branchMgrRole },
            new EmployeeRole { Id = 2, EmployeeId = 90, RoleId = 14, IsAssigned = true, Employee = areaMgr, Role = areaMgrRole },
            new EmployeeRole { Id = 3, EmployeeId = 91, RoleId = 14, IsAssigned = true, Employee = otherAreaMgr, Role = areaMgrRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 14,
            SourceRoleId = 4,
            Role = areaMgrRole,
            IsDeleted = false
        });

        var recipients = await CreateSut().GetOperationalManagersAsync(20);

        Assert.Contains(90, recipients);
        Assert.DoesNotContain(91, recipients);
    }

    [Fact]
    public async Task BranchScope_DoesNotNotify_WhenManagerLacksListeningRole()
    {
        var employeeRole = new Role { Id = 7, Name = "Employee", NotificationScope = NotificationScope.None };
        var branchMgrRole = new Role { Id = 4, Name = "Branch Manager", NotificationScope = NotificationScope.Branch };

        var subject = new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };
        // Manager of branch but only has Employee role — not the listening Branch Manager role
        var manager = new Employee { Id = 20, CompanyId = 1, BranchId = 5, IsActive = true, EmployeeTypeId = 1, EmployeeType = OpsType() };

        SetupRepo(_employeeRepo, subject, manager);
        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 20,
            IsActive = true,
            IsDeleted = false
        });
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo,
            new EmployeeRole { Id = 1, EmployeeId = 10, RoleId = 7, IsAssigned = true, Employee = subject, Role = employeeRole },
            new EmployeeRole { Id = 2, EmployeeId = 20, RoleId = 7, IsAssigned = true, Employee = manager, Role = employeeRole });
        SetupRepo(_notifySourceRepo, new RoleNotificationSource
        {
            Id = 1,
            RoleId = 4,
            SourceRoleId = 7,
            Role = branchMgrRole,
            IsDeleted = false
        });

        var recipients = await CreateSut().GetOperationalManagersAsync(10);
        Assert.DoesNotContain(20, recipients);
    }

    [Fact]
    public async Task IsOrgManagerOverAsync_WhenTargetIsBranchManager_ReturnsTrue()
    {
        SetupRepo(_employeeRepo,
            new Employee { Id = 10, CompanyId = 1, BranchId = 5, IsActive = true },
            new Employee { Id = 20, CompanyId = 1, BranchId = 5, IsActive = true });
        SetupRepo(_branchRepo, new Branch
        {
            Id = 5,
            CompanyId = 1,
            Name = "Branch",
            ManagerID = 20,
            IsDeleted = false
        });
        SetupRepo(_areaRepo);
        SetupRepo(_employeeRoleRepo);
        SetupRepo(_notifySourceRepo);

        Assert.True(await CreateSut().IsOrgManagerOverAsync(10, 20));
    }
}
