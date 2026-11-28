using Microsoft.EntityFrameworkCore;
using Moq;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Services;

namespace TaskMangment.Tests.Security;

public class EmployeePermissionServiceTests
{
    private static AppDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(1);

        return new AppDbContext(options, currentUser.Object);
    }

    private static async Task SeedAsync(AppDbContext db)
    {
        var createTask = new Permission { Id = 1, Code = PermissionCodes.CreateTask, Name = "Create", Description = "Create tasks", IsDeleted = false };
        var viewScoped = new Permission { Id = 2, Code = PermissionCodes.ViewScopedTasks, Name = "Scoped", Description = "View scoped", IsDeleted = false };
        var deletedPerm = new Permission { Id = 3, Code = "OLD_PERM", Name = "Old", Description = "Deleted", IsDeleted = true };

        var role = new Role
        {
            Id = 10,
            CompanyId = 1,
            Name = "Assigner",
            Description = "Test role",
            Level = 50,
            IsDeleted = false,
            RolePermissions =
            [
                new RolePermission { RoleId = 10, PermissionId = 1, IsAssigned = true, IsDeleted = false, Permission = createTask },
                new RolePermission { RoleId = 10, PermissionId = 2, IsAssigned = false, IsDeleted = false, Permission = viewScoped },
                new RolePermission { RoleId = 10, PermissionId = 3, IsAssigned = true, IsDeleted = false, Permission = deletedPerm }
            ]
        };

        db.Permissions.AddRange(createTask, viewScoped, deletedPerm);
        db.Roles.Add(role);
        db.EmployeeRoles.Add(new EmployeeRole
        {
            EmployeeId = 100,
            RoleId = 10,
            IsAssigned = true,
            IsDeleted = false,
            Role = role
        });
        db.EmployeeRoles.Add(new EmployeeRole
        {
            EmployeeId = 200,
            RoleId = 10,
            IsAssigned = false,
            IsDeleted = false,
            Role = role
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task HasAsync_AssignedPermission_ReturnsTrue()
    {
        await using var db = CreateDb(nameof(HasAsync_AssignedPermission_ReturnsTrue));
        await SeedAsync(db);
        var sut = new EmployeePermissionService(db);

        Assert.True(await sut.HasAsync(100, PermissionCodes.CreateTask));
    }

    [Fact]
    public async Task HasAsync_UnassignedRolePermission_ReturnsFalse()
    {
        await using var db = CreateDb(nameof(HasAsync_UnassignedRolePermission_ReturnsFalse));
        await SeedAsync(db);
        var sut = new EmployeePermissionService(db);

        Assert.False(await sut.HasAsync(100, PermissionCodes.ViewScopedTasks));
    }

    [Fact]
    public async Task HasAsync_UnassignedEmployeeRole_ReturnsFalse()
    {
        await using var db = CreateDb(nameof(HasAsync_UnassignedEmployeeRole_ReturnsFalse));
        await SeedAsync(db);
        var sut = new EmployeePermissionService(db);

        Assert.False(await sut.HasAsync(200, PermissionCodes.CreateTask));
    }

    [Fact]
    public async Task GetPermissionsAsync_ReturnsOnlyAssignedActiveCodes()
    {
        await using var db = CreateDb(nameof(GetPermissionsAsync_ReturnsOnlyAssignedActiveCodes));
        await SeedAsync(db);
        var sut = new EmployeePermissionService(db);

        var perms = await sut.GetPermissionsAsync(100);

        Assert.Contains(PermissionCodes.CreateTask, perms);
        Assert.DoesNotContain(PermissionCodes.ViewScopedTasks, perms);
        Assert.DoesNotContain("OLD_PERM", perms);
    }

    [Fact]
    public async Task HasAnyAsync_WhenOneMatches_ReturnsTrue()
    {
        await using var db = CreateDb(nameof(HasAnyAsync_WhenOneMatches_ReturnsTrue));
        await SeedAsync(db);
        var sut = new EmployeePermissionService(db);

        Assert.True(await sut.HasAnyAsync(100, PermissionCodes.ViewCompanyTasks, PermissionCodes.CreateTask));
    }
}
