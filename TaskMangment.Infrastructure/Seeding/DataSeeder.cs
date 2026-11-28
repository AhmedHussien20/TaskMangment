using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Common.Security;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Seeding
{
    public class DataSeeder
    {
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<RolePermission> _rolePermRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            IRepository<Permission> permissionRepo,
            IRepository<Role> roleRepo,
            IRepository<RolePermission> rolePermRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            ILogger<DataSeeder> logger)
        {
            _permissionRepo = permissionRepo;
            _roleRepo = roleRepo;
            _rolePermRepo = rolePermRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding process...");

                await SeedPermissionsAsync();
                await SeedRolesAsync();
                await AssignPermissionsToRolesAsync();

                _logger.LogInformation("Data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during data seeding");
                throw; 
            }
        }

        private async Task SeedPermissionsAsync()
        {
            //if (!await _permissionrepo.getall().anyasync())
            //{
                _logger.LogInformation("Seeding permissions...");

            var permissions = PermissionCatalog.All.Select(e => new Permission
            {
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                CreatedDate = DateTime.UtcNow
            }).ToList();
            // إضافة جميع الصلاحيات دفعة واحدة
            await _permissionRepo.AddRangeAsync(permissions);
                await _permissionRepo.SaveChangesAsync();

                _logger.LogInformation($"Seeded {permissions.Count} permissions.");
            //}
            //else
            //{
            //    _logger.LogInformation("Permissions already exist. Skipping...");
            //}
        }

        private async Task SeedRolesAsync()
        {
            //if (!await _roleRepo.GetAll().AnyAsync())
            //{
                _logger.LogInformation("Seeding roles...");

                var roles = new List<Role>
                {
                     new Role
                    {
                        Name = "Manager",
                        CompanyId = 16,
                        Description = "Manager company responsible for whole things",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Technical Support",
                        CompanyId = 16, 
                        Description = "Handles technical issues and support requests",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "CEO",
                        CompanyId = 16,
                        Description = "Chief Executive Officer with all permissions",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Branch Manager",
                        CompanyId = 16,
                        Description = "Manages branch operations and employees",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Group 10",
                        CompanyId = 16,
                        Description = "Special group with specific permissions",
                        CreatedDate = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "UV",
                        CompanyId = 16,
                        Description = "UV group role",
                        CreatedDate = DateTime.UtcNow
                    }
                };

                await _roleRepo.AddRangeAsync(roles);
                await _roleRepo.SaveChangesAsync();

                _logger.LogInformation($"Seeded {roles.Count} roles.");
            //}
            //else
            //{
            //    _logger.LogInformation("Roles already exist. Skipping...");
            //}
        }

        private async Task AssignPermissionsToRolesAsync()
        {
            _logger.LogInformation("Assigning permissions to roles...");

            var fullAccessRoles = await _roleRepo
                .GetAll(r => r.Name == "CEO" || r.Name == "Manager")
                .ToListAsync();

            var allPermissions = await _permissionRepo.GetAll().ToListAsync();

            foreach (var role in fullAccessRoles)
            {
                var existingPermissions = await _rolePermRepo
                    .GetAll(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                var newPermissions = allPermissions
                    .Where(p => !existingPermissions.Contains(p.Id))
                    .Select(p => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = p.Id,
                        CreatedDate = DateTime.UtcNow
                    });

                if (newPermissions.Any())
                {
                    await _rolePermRepo.AddRangeAsync(newPermissions);
                    await _rolePermRepo.SaveChangesAsync();
                    _logger.LogInformation($"Assigned {newPermissions.Count()} permissions to {role.Name} role.");
                }
            }

            _logger.LogInformation("Permission assignment completed for full access roles.");
        }
    }
}
