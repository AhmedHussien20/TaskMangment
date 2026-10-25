using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class PlatformService : IPlatformService
    {
        private readonly AppDbContext _dbo;
        private readonly TrainingDbContext _training;
        private readonly IJwtService _jwt;

        public PlatformService(AppDbContext dbo, TrainingDbContext training, IJwtService jwt)
        {
            _dbo = dbo;
            _training = training;
            _jwt = jwt;
        }

        public async Task<ApiResponse<PlatformLoginResponse>> LoginAsync(PlatformLoginRequest request)
        {
            var email = (request.Email ?? string.Empty).Trim();
            var user = await _dbo.PlatformUsers
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password ?? string.Empty, user.PasswordHash))
                throw new AppException(ErrorCodes.Invalid, StatusCodes.Status400BadRequest);

            if (!user.IsActive)
                throw new AppException(ErrorCodes.EmployeeInactive, StatusCodes.Status403Forbidden);

            if (user.Role == PlatformRole.OrganizationAdmin &&
                (user.Organization == null || !user.Organization.IsActive || user.Organization.IsDeleted))
                throw new AppException(ErrorCodes.EmployeeInactive, StatusCodes.Status403Forbidden);

            return ApiResponse<PlatformLoginResponse>.Ok(new PlatformLoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization?.Name,
                Token = _jwt.GeneratePlatformToken(user)
            });
        }

        public async Task<ApiResponse<List<OrganizationListItemDto>>> GetOrganizationsAsync(int platformUserId)
        {
            var actor = await RequirePlatformAdminAsync(platformUserId);

            var items = await _dbo.Organizations
                .AsNoTracking()
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.Id)
                .Select(o => new OrganizationListItemDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    IsActive = o.IsActive,
                    CreatedDate = o.CreatedDate,
                    AdminEmail = o.Admins
                        .Where(a => !a.IsDeleted && a.Role == PlatformRole.OrganizationAdmin)
                        .OrderBy(a => a.Id)
                        .Select(a => a.Email)
                        .FirstOrDefault(),
                    AdminFullName = o.Admins
                        .Where(a => !a.IsDeleted && a.Role == PlatformRole.OrganizationAdmin)
                        .OrderBy(a => a.Id)
                        .Select(a => a.FullName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return ApiResponse<List<OrganizationListItemDto>>.Ok(items);
        }

        public async Task<ApiResponse<OrganizationListItemDto>> CreateOrganizationAsync(int platformUserId, CreateOrganizationRequest request)
        {
            await RequirePlatformAdminAsync(platformUserId);

            var name = (request.Name ?? string.Empty).Trim();
            var adminName = (request.AdminFullName ?? string.Empty).Trim();
            var adminEmail = (request.AdminEmail ?? string.Empty).Trim();
            var password = request.AdminPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminEmail) || password.Length < 6)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            if (await EmailTakenAsync(adminEmail))
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            var organization = new Organization
            {
                Name = name,
                IsActive = true,
                CreatedBy = platformUserId
            };

            var admin = new PlatformUser
            {
                FullName = adminName,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = PlatformRole.OrganizationAdmin,
                IsActive = true,
                Organization = organization,
                CreatedBy = platformUserId
            };

            _dbo.Organizations.Add(organization);
            _dbo.PlatformUsers.Add(admin);
            await _dbo.SaveChangesAsync();

            return ApiResponse<OrganizationListItemDto>.Ok(new OrganizationListItemDto
            {
                Id = organization.Id,
                Name = organization.Name,
                IsActive = organization.IsActive,
                CreatedDate = organization.CreatedDate,
                AdminEmail = admin.Email,
                AdminFullName = admin.FullName
            });
        }

        public async Task<ApiResponse<List<TraineeCopyDto>>> GetTraineesAsync(int platformUserId)
        {
            var actor = await RequireOrganizationAdminAsync(platformUserId);
            var organizationId = actor.OrganizationId!.Value;

            var companies = await _training.Companies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsDeleted && c.OrganizationId == organizationId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            var companyIds = companies.Select(c => c.Id).ToList();
            var admins = await _training.Employees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(e => companyIds.Contains(e.CompanyId) && !e.IsDeleted)
                .OrderBy(e => e.Id)
                .ToListAsync();

            var items = companies.Select(c =>
            {
                var admin = admins.FirstOrDefault(e => e.CompanyId == c.Id);
                return new TraineeCopyDto
                {
                    CompanyId = c.Id,
                    CompanyName = c.Name,
                    AdminEmail = admin?.Email ?? string.Empty,
                    AdminFullName = admin?.FullName ?? string.Empty,
                    IsActive = c.IsActive && (admin?.IsActive ?? false),
                    CreatedDate = c.CreatedDate,
                    OrganizationId = organizationId
                };
            }).ToList();

            return ApiResponse<List<TraineeCopyDto>>.Ok(items);
        }

        public async Task<ApiResponse<TraineeCopyDto>> CreateTraineeAsync(int platformUserId, CreateTraineeRequest request)
        {
            var actor = await RequireOrganizationAdminAsync(platformUserId);
            var organizationId = actor.OrganizationId!.Value;

            var companyName = (request.CompanyName ?? string.Empty).Trim();
            var adminName = (request.AdminFullName ?? string.Empty).Trim();
            var adminEmail = (request.AdminEmail ?? string.Empty).Trim();
            var password = request.AdminPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminEmail) || password.Length < 6)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            if (await EmailTakenAsync(adminEmail))
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            var employeeTypeId = await _training.EmployeeTypes
                .IgnoreQueryFilters()
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Id)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();

            if (employeeTypeId == 0)
                throw new AppException(ErrorCodes.SaveFailed, StatusCodes.Status500InternalServerError);

            var permissions = await _training.Permissions
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync();

            await using var tx = await _training.Database.BeginTransactionAsync();

            var company = new Company
            {
                Name = companyName,
                Address = string.Empty,
                Phone = string.Empty,
                Mobile = string.Empty,
                Fax = string.Empty,
                Email = adminEmail,
                Website = string.Empty,
                CommercialRecord = string.Empty,
                IsActive = true,
                OrganizationId = organizationId,
                CreatedBy = platformUserId
            };
            _training.Companies.Add(company);
            await _training.SaveChangesAsync();

            var employee = new Employee
            {
                CompanyId = company.Id,
                FullName = adminName,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                EmployeeTypeId = employeeTypeId,
                Title = "مدير الشركة",
                Nationality = string.Empty,
                IdentityNumber = string.Empty,
                Mobile = string.Empty,
                Address = string.Empty,
                Qualification = string.Empty,
                CreatedBy = platformUserId
            };
            _training.Employees.Add(employee);

            var role = new Role
            {
                CompanyId = company.Id,
                Name = "Company Admin",
                Description = "مدير نسخة المتدرب",
                Level = (int)RoleLevelEnum.Admin,
                CreatedBy = platformUserId
            };
            _training.Roles.Add(role);
            await _training.SaveChangesAsync();

            _training.RolePermissions.AddRange(permissions.Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
                IsAssigned = true,
                CreatedBy = platformUserId
            }));

            _training.EmployeeRoles.Add(new EmployeeRole
            {
                EmployeeId = employee.Id,
                RoleId = role.Id,
                IsAssigned = true,
                CreatedBy = platformUserId
            });

            await _training.SaveChangesAsync();
            await tx.CommitAsync();

            return ApiResponse<TraineeCopyDto>.Ok(new TraineeCopyDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                AdminEmail = employee.Email,
                AdminFullName = employee.FullName,
                IsActive = true,
                CreatedDate = company.CreatedDate,
                OrganizationId = organizationId
            });
        }

        public async Task<ApiResponse<TraineeCopyDto>> SetTraineeActiveAsync(int platformUserId, int companyId, bool isActive)
        {
            var actor = await RequireOrganizationAdminAsync(platformUserId);
            var organizationId = actor.OrganizationId!.Value;

            var company = await _training.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted && c.OrganizationId == organizationId);

            if (company == null)
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status404NotFound);

            var employees = await _training.Employees
                .IgnoreQueryFilters()
                .Where(e => e.CompanyId == companyId && !e.IsDeleted)
                .ToListAsync();

            company.IsActive = isActive;
            foreach (var employee in employees)
                employee.IsActive = isActive;

            await _training.SaveChangesAsync();

            var admin = employees.OrderBy(e => e.Id).FirstOrDefault();
            return ApiResponse<TraineeCopyDto>.Ok(new TraineeCopyDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                AdminEmail = admin?.Email ?? string.Empty,
                AdminFullName = admin?.FullName ?? string.Empty,
                IsActive = isActive,
                CreatedDate = company.CreatedDate,
                OrganizationId = organizationId
            });
        }

        private async Task<PlatformUser> RequirePlatformAdminAsync(int platformUserId)
        {
            var user = await _dbo.PlatformUsers.FirstOrDefaultAsync(u => u.Id == platformUserId && !u.IsDeleted && u.IsActive);
            if (user == null || user.Role != PlatformRole.PlatformAdmin)
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);
            return user;
        }

        private async Task<PlatformUser> RequireOrganizationAdminAsync(int platformUserId)
        {
            var user = await _dbo.PlatformUsers
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Id == platformUserId && !u.IsDeleted && u.IsActive);

            if (user == null || user.Role != PlatformRole.OrganizationAdmin || user.OrganizationId == null)
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);

            if (user.Organization == null || !user.Organization.IsActive || user.Organization.IsDeleted)
                throw new AppException(ErrorCodes.EmployeeInactive, StatusCodes.Status403Forbidden);

            return user;
        }

        private async Task<bool> EmailTakenAsync(string email)
        {
            if (await _dbo.PlatformUsers.AnyAsync(u => u.Email == email && !u.IsDeleted))
                return true;
            if (await _dbo.Employees.AnyAsync(e => e.Email == email && !e.IsDeleted))
                return true;
            return await _training.Employees.IgnoreQueryFilters().AnyAsync(e => e.Email == email && !e.IsDeleted);
        }
    }
}
