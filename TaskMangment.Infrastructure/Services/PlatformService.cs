using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.ApiRequests.Platform;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Persistence.Extensions;

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

        public async Task<ApiResponse<PagedResponse<OrganizationListItemDto>>> GetOrganizationsAsync(int platformUserId, OrganizationListRequest request)
        {
            await RequireInstituteStaffAsync(platformUserId);
            var (pageIndex, pageSize) = NormalizePage(request);

            var query = _dbo.Organizations
                .AsNoTracking()
                .Where(o => !o.IsDeleted);

            if (request.IsActive.HasValue)
                query = query.Where(o => o.IsActive == request.IsActive.Value);

            var term = (request.searchKey ?? string.Empty).Trim();
            if (term.Length > 0)
            {
                query = query.Where(o =>
                    o.Name.Contains(term) ||
                    o.Admins.Any(a => !a.IsDeleted && a.Role == PlatformRole.OrganizationAdmin &&
                        (a.FullName.Contains(term) || a.Email.Contains(term))));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDynamicSafe(OrganizationSortColumn(request.SortColumn), request.SortDirection)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
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

            return ApiResponse<PagedResponse<OrganizationListItemDto>>.Ok(
                new PagedResponse<OrganizationListItemDto>(items, total, pageIndex, pageSize));
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

        public async Task<ApiResponse<PagedResponse<TraineeCopyDto>>> GetTraineesAsync(int platformUserId, TraineeCopyListRequest request)
        {
            var scope = await RequireTraineeActorAsync(platformUserId);
            var (pageIndex, pageSize) = NormalizePage(request);

            var companies = await _training.Companies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsDeleted && (scope == null || c.OrganizationId == scope))
                .ToListAsync();

            var companyIds = companies.Select(c => c.Id).ToList();
            var admins = await _training.Employees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(e => companyIds.Contains(e.CompanyId) && !e.IsDeleted)
                .OrderBy(e => e.Id)
                .ToListAsync();

            var organizationIds = companies
                .Where(c => c.OrganizationId.HasValue)
                .Select(c => c.OrganizationId!.Value)
                .Distinct()
                .ToList();
            var organizationNames = await _dbo.Organizations
                .AsNoTracking()
                .Where(o => organizationIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.Name);

            var items = companies.Select(c =>
            {
                var admin = admins.FirstOrDefault(e => e.CompanyId == c.Id);
                var organizationId = c.OrganizationId ?? 0;
                return new TraineeCopyDto
                {
                    CompanyId = c.Id,
                    CompanyName = c.Name,
                    AdminEmail = admin?.Email ?? string.Empty,
                    AdminFullName = admin?.FullName ?? string.Empty,
                    IsActive = c.IsActive && (admin?.IsActive ?? false),
                    CreatedDate = c.CreatedDate,
                    OrganizationId = organizationId,
                    OrganizationName = organizationNames.GetValueOrDefault(organizationId) ?? string.Empty
                };
            }).ToList();

            var term = (request.searchKey ?? string.Empty).Trim();
            if (term.Length > 0)
            {
                items = items.Where(row =>
                    row.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    row.AdminFullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    row.AdminEmail.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    row.OrganizationName.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (request.IsActive.HasValue)
                items = items.Where(row => row.IsActive == request.IsActive.Value).ToList();

            var descending = !string.Equals(request.SortDirection, "ASC", StringComparison.OrdinalIgnoreCase);
            items = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
            {
                "companyname" => Order(items, row => row.CompanyName, descending),
                "adminfullname" => Order(items, row => row.AdminFullName, descending),
                "adminemail" => Order(items, row => row.AdminEmail, descending),
                "organizationname" => Order(items, row => row.OrganizationName, descending),
                "createddate" => Order(items, row => row.CreatedDate, descending),
                "isactive" => Order(items, row => row.IsActive, descending),
                _ => Order(items, row => row.CompanyId, descending)
            };

            var total = items.Count;
            var page = items.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return ApiResponse<PagedResponse<TraineeCopyDto>>.Ok(
                new PagedResponse<TraineeCopyDto>(page, total, pageIndex, pageSize));
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
            var scope = await RequireTraineeActorAsync(platformUserId);

            var company = await _training.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted && (scope == null || c.OrganizationId == scope));

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
                OrganizationId = company.OrganizationId ?? 0,
                OrganizationName = company.OrganizationId.HasValue
                    ? await _dbo.Organizations.Where(o => o.Id == company.OrganizationId).Select(o => o.Name).FirstOrDefaultAsync() ?? string.Empty
                    : string.Empty
            });
        }

        private async Task<int?> RequireTraineeActorAsync(int platformUserId)
        {
            var user = await _dbo.PlatformUsers
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Id == platformUserId && !u.IsDeleted && u.IsActive);

            if (user == null)
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);

            if (user.Role == PlatformRole.PlatformAdmin || user.Role == PlatformRole.SuperAdmin)
                return null;

            if (user.Role != PlatformRole.OrganizationAdmin || user.OrganizationId == null ||
                user.Organization == null || !user.Organization.IsActive || user.Organization.IsDeleted)
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);

            return user.OrganizationId;
        }

        public async Task<ApiResponse<PagedResponse<PlatformAdminListItemDto>>> GetPlatformAdminsAsync(int platformUserId, PlatformAdminListRequest request)
        {
            await RequireSuperAdminAsync(platformUserId);
            var (pageIndex, pageSize) = NormalizePage(request);

            var query = _dbo.PlatformUsers
                .AsNoTracking()
                .Where(u => !u.IsDeleted && u.Role == PlatformRole.PlatformAdmin);

            if (request.IsActive.HasValue)
                query = query.Where(u => u.IsActive == request.IsActive.Value);

            var term = (request.searchKey ?? string.Empty).Trim();
            if (term.Length > 0)
                query = query.Where(u => u.FullName.Contains(term) || u.Email.Contains(term));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDynamicSafe(AdminSortColumn(request.SortColumn), request.SortDirection)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new PlatformAdminListItemDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate
                })
                .ToListAsync();

            return ApiResponse<PagedResponse<PlatformAdminListItemDto>>.Ok(
                new PagedResponse<PlatformAdminListItemDto>(items, total, pageIndex, pageSize));
        }

        public async Task<ApiResponse<PlatformAdminListItemDto>> CreatePlatformAdminAsync(int platformUserId, CreatePlatformAdminRequest request)
        {
            await RequireSuperAdminAsync(platformUserId);

            var name = (request.FullName ?? string.Empty).Trim();
            var email = (request.Email ?? string.Empty).Trim();
            var password = request.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || password.Length < 6)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            if (await EmailTakenAsync(email))
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            var admin = new PlatformUser
            {
                FullName = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = PlatformRole.PlatformAdmin,
                IsActive = true,
                CreatedBy = platformUserId
            };

            _dbo.PlatformUsers.Add(admin);
            await _dbo.SaveChangesAsync();

            return ApiResponse<PlatformAdminListItemDto>.Ok(ToAdminDto(admin));
        }

        public async Task<ApiResponse<PlatformAdminListItemDto>> SetPlatformAdminActiveAsync(int platformUserId, int adminId, bool isActive)
        {
            await RequireSuperAdminAsync(platformUserId);

            var admin = await _dbo.PlatformUsers
                .FirstOrDefaultAsync(u => u.Id == adminId && !u.IsDeleted && u.Role == PlatformRole.PlatformAdmin);

            if (admin == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            admin.IsActive = isActive;
            await _dbo.SaveChangesAsync();

            return ApiResponse<PlatformAdminListItemDto>.Ok(ToAdminDto(admin));
        }

        private static (int PageIndex, int PageSize) NormalizePage(BaseApiRequest request)
        {
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            return (pageIndex, pageSize);
        }

        private static string OrganizationSortColumn(string? column) => (column ?? string.Empty).ToLowerInvariant() switch
        {
            "name" => "Name",
            "createddate" => "CreatedDate",
            "isactive" => "IsActive",
            _ => "Id"
        };

        private static string AdminSortColumn(string? column) => (column ?? string.Empty).ToLowerInvariant() switch
        {
            "fullname" => "FullName",
            "email" => "Email",
            "createddate" => "CreatedDate",
            "isactive" => "IsActive",
            _ => "Id"
        };

        private static List<T> Order<T, TKey>(List<T> items, Func<T, TKey> key, bool descending)
        {
            return descending
                ? items.OrderByDescending(key).ToList()
                : items.OrderBy(key).ToList();
        }

        private static PlatformAdminListItemDto ToAdminDto(PlatformUser admin) => new()
        {
            Id = admin.Id,
            FullName = admin.FullName,
            Email = admin.Email,
            IsActive = admin.IsActive,
            CreatedDate = admin.CreatedDate
        };

        private async Task<PlatformUser> RequireSuperAdminAsync(int platformUserId)
        {
            var user = await _dbo.PlatformUsers.FirstOrDefaultAsync(u => u.Id == platformUserId && !u.IsDeleted && u.IsActive);
            if (user == null || user.Role != PlatformRole.SuperAdmin)
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);
            return user;
        }

        private async Task<PlatformUser> RequireInstituteStaffAsync(int platformUserId)
        {
            var user = await _dbo.PlatformUsers.FirstOrDefaultAsync(u => u.Id == platformUserId && !u.IsDeleted && u.IsActive);
            if (user == null || (user.Role != PlatformRole.PlatformAdmin && user.Role != PlatformRole.SuperAdmin))
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);
            return user;
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
