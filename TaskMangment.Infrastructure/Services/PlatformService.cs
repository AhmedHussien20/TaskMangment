using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
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
        private readonly OperationalEmailRegistry _emails;

        public PlatformService(AppDbContext dbo, TrainingDbContext training, IJwtService jwt, OperationalEmailRegistry emails)
        {
            _dbo = dbo;
            _training = training;
            _jwt = jwt;
            _emails = emails;
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
                    AdminId = o.Admins
                        .Where(a => !a.IsDeleted && a.Role == PlatformRole.OrganizationAdmin)
                        .OrderBy(a => a.Id)
                        .Select(a => (int?)a.Id)
                        .FirstOrDefault(),
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
            await SaveDboAsync();

            return ApiResponse<OrganizationListItemDto>.Ok(ToOrganizationDto(organization, admin));
        }

        public async Task<ApiResponse<OrganizationListItemDto>> UpdateOrganizationAsync(
            int platformUserId, int organizationId, UpdateOrganizationRequest request)
        {
            await RequirePlatformAdminAsync(platformUserId);

            var name = (request.Name ?? string.Empty).Trim();
            var adminName = (request.AdminFullName ?? string.Empty).Trim();
            var adminEmail = (request.AdminEmail ?? string.Empty).Trim();
            var password = request.AdminPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminEmail))
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            if (password.Length > 0 && password.Length < 6)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            var organization = await _dbo.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted);

            if (organization == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var admin = await _dbo.PlatformUsers
                .Where(a => a.OrganizationId == organizationId && !a.IsDeleted && a.Role == PlatformRole.OrganizationAdmin)
                .OrderBy(a => a.Id)
                .FirstOrDefaultAsync();

            if (admin == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (await EmailTakenAsync(adminEmail, exceptPlatformUserId: admin.Id))
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            organization.Name = name;
            organization.ModifiedBy = platformUserId;

            admin.FullName = adminName;
            admin.Email = adminEmail;
            admin.ModifiedBy = platformUserId;
            if (password.Length >= 6)
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await SaveDboAsync();
            return ApiResponse<OrganizationListItemDto>.Ok(ToOrganizationDto(organization, admin));
        }

        public async Task<ApiResponse<bool>> DeleteOrganizationAsync(int platformUserId, int organizationId)
        {
            await RequirePlatformAdminAsync(platformUserId);

            var organization = await _dbo.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted);

            if (organization == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            SoftDelete(organization, platformUserId);

            var admins = await _dbo.PlatformUsers
                .Where(a => a.OrganizationId == organizationId && !a.IsDeleted)
                .ToListAsync();
            foreach (var admin in admins)
                SoftDelete(admin, platformUserId);

            var companies = await _training.Companies
                .IgnoreQueryFilters()
                .Where(c => c.OrganizationId == organizationId && !c.IsDeleted)
                .ToListAsync();

            foreach (var company in companies)
                await SoftDeleteTraineeCompanyAsync(company, platformUserId);

            await SaveDboAsync();
            await _training.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true);
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
            var organizationId = await ResolveTraineeOrganizationIdAsync(platformUserId, request.OrganizationId);

            var companyName = (request.CompanyName ?? string.Empty).Trim();
            var adminName = (request.AdminFullName ?? string.Empty).Trim();
            var adminEmail = (request.AdminEmail ?? string.Empty).Trim();
            var password = request.AdminPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminEmail) || password.Length < 6)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            if (await EmailTakenAsync(adminEmail))
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            // Templates (CompanyId 0) are copied into the new copy so it owns, and can change, its own catalogs.
            var typeTemplates = await _training.EmployeeTypes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(t => t.CompanyId == 0 && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .ToListAsync();

            if (typeTemplates.Count == 0)
                throw new AppException(ErrorCodes.SaveFailed, StatusCodes.Status500InternalServerError);

            var permissionTemplates = await _training.Permissions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.CompanyId == 0 && !p.IsDeleted)
                .OrderBy(p => p.Id)
                .ToListAsync();

            var leaveTypeTemplates = await _training.LeaveTypes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(l => l.CompanyId == 0 && !l.IsDeleted)
                .OrderBy(l => l.Id)
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

            var employeeTypes = typeTemplates.Select(t => new EmployeeType
            {
                CompanyId = company.Id,
                Code = t.Code,
                NameEn = t.NameEn,
                NameAr = t.NameAr,
                SeesAllTypesInBranchScope = t.SeesAllTypesInBranchScope,
                CreatedBy = platformUserId
            }).ToList();
            var permissions = permissionTemplates.Select(p => new Permission
            {
                CompanyId = company.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                CreatedBy = platformUserId
            }).ToList();
            _training.EmployeeTypes.AddRange(employeeTypes);
            _training.Permissions.AddRange(permissions);
            _training.LeaveTypes.AddRange(leaveTypeTemplates.Select(l => new LeaveType
            {
                CompanyId = company.Id,
                NameAr = l.NameAr,
                NameEn = l.NameEn,
                IsPaid = l.IsPaid,
                MaxDaysPerYear = l.MaxDaysPerYear,
                CreatedBy = platformUserId
            }));
            await _training.SaveChangesAsync();

            var employeeTypeId = employeeTypes[0].Id;

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
                // So the seeded admin can own the main branch and create more branches later.
                CanBeBranchManager = true,
                CreatedBy = platformUserId
            };
            _training.Roles.Add(role);
            await _training.SaveChangesAsync();

            _training.RolePermissions.AddRange(permissions.Select(permission => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id,
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

            // Bootstrap org tree: empty trainee companies cannot create branches (need a manager)
            // or employees that expect a branch. Seed one main branch owned by the company admin.
            var mainBranch = new Branch
            {
                CompanyId = company.Id,
                Name = "الفرع الرئيسي",
                Address = string.Empty,
                Phone = string.Empty,
                Mobile = string.Empty,
                Fax = string.Empty,
                Email = adminEmail,
                ManagerID = employee.Id,
                ResponsibleID = employee.Id,
                IsActive = true,
                MainBranch = true,
                CreatedBy = platformUserId
            };
            _training.Branches.Add(mainBranch);
            await _training.SaveChangesAsync();

            var mainDepartment = new Department
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                Name = "الإدارة الرئيسية",
                ManagerEmployeeId = employee.Id,
                CreatedBy = platformUserId
            };
            _training.Departments.Add(mainDepartment);
            await _training.SaveChangesAsync();

            employee.BranchId = mainBranch.Id;
            employee.DepartmentId = mainDepartment.Id;
            employee.ModifiedBy = platformUserId;

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

        public async Task<ApiResponse<TraineeCopyDto>> UpdateTraineeAsync(
            int platformUserId, int companyId, UpdateTraineeRequest request)
        {
            var scope = await RequireTraineeActorAsync(platformUserId);

            var companyName = (request.CompanyName ?? string.Empty).Trim();
            var adminName = (request.AdminFullName ?? string.Empty).Trim();
            var adminEmail = (request.AdminEmail ?? string.Empty).Trim();
            var password = request.AdminPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminEmail))
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            if (password.Length > 0 && password.Length < 6)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            var company = await _training.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted && (scope == null || c.OrganizationId == scope));

            if (company == null)
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status404NotFound);

            var admin = await _training.Employees
                .IgnoreQueryFilters()
                .Where(e => e.CompanyId == companyId && !e.IsDeleted)
                .OrderBy(e => e.Id)
                .FirstOrDefaultAsync();

            if (admin == null)
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status404NotFound);

            if (await EmailTakenAsync(adminEmail, exceptTrainingEmployeeId: admin.Id))
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            company.Name = companyName;
            company.Email = adminEmail;
            company.ModifiedBy = platformUserId;

            admin.FullName = adminName;
            admin.Email = adminEmail;
            admin.ModifiedBy = platformUserId;
            if (password.Length >= 6)
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await _training.SaveChangesAsync();

            return ApiResponse<TraineeCopyDto>.Ok(await ToTraineeDtoAsync(company, admin));
        }

        public async Task<ApiResponse<bool>> DeleteTraineeAsync(int platformUserId, int companyId)
        {
            var scope = await RequireTraineeActorAsync(platformUserId);

            var company = await _training.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted && (scope == null || c.OrganizationId == scope));

            if (company == null)
                throw new AppException(ErrorCodes.CompanyNotFound, StatusCodes.Status404NotFound);

            await SoftDeleteTraineeCompanyAsync(company, platformUserId);
            await _training.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true);
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
            return ApiResponse<TraineeCopyDto>.Ok(await ToTraineeDtoAsync(company, admin, isActive));
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
            await SaveDboAsync();

            return ApiResponse<PlatformAdminListItemDto>.Ok(ToAdminDto(admin));
        }

        public async Task<ApiResponse<PlatformAdminListItemDto>> UpdatePlatformAdminAsync(
            int platformUserId, int adminId, UpdatePlatformAdminRequest request)
        {
            await RequireSuperAdminAsync(platformUserId);

            var name = (request.FullName ?? string.Empty).Trim();
            var email = (request.Email ?? string.Empty).Trim();
            var password = request.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            if (password.Length > 0 && password.Length < 6)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            var admin = await _dbo.PlatformUsers
                .FirstOrDefaultAsync(u => u.Id == adminId && !u.IsDeleted && u.Role == PlatformRole.PlatformAdmin);

            if (admin == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (await EmailTakenAsync(email, exceptPlatformUserId: admin.Id))
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);

            admin.FullName = name;
            admin.Email = email;
            admin.ModifiedBy = platformUserId;
            if (password.Length >= 6)
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await SaveDboAsync();
            return ApiResponse<PlatformAdminListItemDto>.Ok(ToAdminDto(admin));
        }

        public async Task<ApiResponse<bool>> DeletePlatformAdminAsync(int platformUserId, int adminId)
        {
            await RequireSuperAdminAsync(platformUserId);

            var admin = await _dbo.PlatformUsers
                .FirstOrDefaultAsync(u => u.Id == adminId && !u.IsDeleted && u.Role == PlatformRole.PlatformAdmin);

            if (admin == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            SoftDelete(admin, platformUserId);
            await SaveDboAsync();

            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<PlatformAdminListItemDto>> SetPlatformAdminActiveAsync(int platformUserId, int adminId, bool isActive)
        {
            await RequireSuperAdminAsync(platformUserId);

            var admin = await _dbo.PlatformUsers
                .FirstOrDefaultAsync(u => u.Id == adminId && !u.IsDeleted && u.Role == PlatformRole.PlatformAdmin);

            if (admin == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            admin.IsActive = isActive;
            await SaveDboAsync();

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

        private static OrganizationListItemDto ToOrganizationDto(Organization organization, PlatformUser admin) => new()
        {
            Id = organization.Id,
            Name = organization.Name,
            IsActive = organization.IsActive,
            CreatedDate = organization.CreatedDate,
            AdminId = admin.Id,
            AdminEmail = admin.Email,
            AdminFullName = admin.FullName
        };

        private async Task<TraineeCopyDto> ToTraineeDtoAsync(Company company, Employee? admin, bool? isActiveOverride = null)
        {
            var organizationId = company.OrganizationId ?? 0;
            return new TraineeCopyDto
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                AdminEmail = admin?.Email ?? string.Empty,
                AdminFullName = admin?.FullName ?? string.Empty,
                IsActive = isActiveOverride ?? (company.IsActive && (admin?.IsActive ?? false)),
                CreatedDate = company.CreatedDate,
                OrganizationId = organizationId,
                OrganizationName = organizationId > 0
                    ? await _dbo.Organizations.Where(o => o.Id == organizationId).Select(o => o.Name).FirstOrDefaultAsync() ?? string.Empty
                    : string.Empty
            };
        }

        private async Task SoftDeleteTraineeCompanyAsync(Company company, int platformUserId)
        {
            SoftDelete(company, platformUserId);
            company.IsActive = false;

            var employees = await _training.Employees
                .IgnoreQueryFilters()
                .Where(e => e.CompanyId == company.Id && !e.IsDeleted)
                .ToListAsync();

            foreach (var employee in employees)
            {
                SoftDelete(employee, platformUserId);
                employee.IsActive = false;
            }
        }

        private static void SoftDelete(BaseEntity entity, int platformUserId)
        {
            entity.IsDeleted = true;
            entity.DeletedBy = platformUserId;
            entity.DeletedDate = DateTime.UtcNow;
        }

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

        /// <summary>PlatformAdmin or SuperAdmin may manage institutes (create/edit/delete).</summary>
        private async Task<PlatformUser> RequirePlatformAdminAsync(int platformUserId)
        {
            var user = await _dbo.PlatformUsers.FirstOrDefaultAsync(u => u.Id == platformUserId && !u.IsDeleted && u.IsActive);
            if (user == null ||
                (user.Role != PlatformRole.PlatformAdmin && user.Role != PlatformRole.SuperAdmin))
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

        /// <summary>
        /// Org admin → own organization. SuperAdmin / PlatformAdmin → request.OrganizationId required.
        /// </summary>
        private async Task<int> ResolveTraineeOrganizationIdAsync(int platformUserId, int? requestedOrganizationId)
        {
            var user = await _dbo.PlatformUsers
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Id == platformUserId && !u.IsDeleted && u.IsActive);

            if (user == null)
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);

            if (user.Role == PlatformRole.OrganizationAdmin)
            {
                if (user.OrganizationId == null ||
                    user.Organization == null || !user.Organization.IsActive || user.Organization.IsDeleted)
                    throw new AppException(ErrorCodes.EmployeeInactive, StatusCodes.Status403Forbidden);
                return user.OrganizationId.Value;
            }

            if (user.Role != PlatformRole.PlatformAdmin && user.Role != PlatformRole.SuperAdmin)
                throw new AppException(ErrorCodes.NotAuthorized, StatusCodes.Status403Forbidden);

            if (!requestedOrganizationId.HasValue || requestedOrganizationId.Value <= 0)
                throw new AppException(ErrorCodes.ValidationError, StatusCodes.Status400BadRequest);

            var orgExists = await _dbo.Organizations.AnyAsync(o =>
                o.Id == requestedOrganizationId.Value && !o.IsDeleted && o.IsActive);
            if (!orgExists)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            return requestedOrganizationId.Value;
        }

        private Task<bool> EmailTakenAsync(
            string email,
            int? exceptPlatformUserId = null,
            int? exceptTrainingEmployeeId = null) =>
            _emails.IsTakenAsync(
                email,
                exceptPlatformUserId: exceptPlatformUserId,
                exceptTrainingEmployeeId: exceptTrainingEmployeeId);

        private async Task SaveDboAsync()
        {
            try
            {
                await _dbo.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsDuplicateEmailConstraint(ex))
            {
                throw new AppException(ErrorCodes.EmailAlreadyExists, StatusCodes.Status400BadRequest);
            }
        }

        private static bool IsDuplicateEmailConstraint(DbUpdateException ex)
        {
            if (ex.InnerException is not SqlException sql)
                return false;

            // 2601 = unique index, 2627 = unique/primary key constraint
            if (sql.Number is not (2601 or 2627))
                return false;

            var message = sql.Message ?? string.Empty;
            return message.Contains("IX_PlatformUsers_Email", StringComparison.OrdinalIgnoreCase)
                || message.Contains("PlatformUsers", StringComparison.OrdinalIgnoreCase)
                || message.Contains("Email", StringComparison.OrdinalIgnoreCase);
        }
    }
}
