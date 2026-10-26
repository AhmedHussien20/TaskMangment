using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Linq;
using TaskMangment.Application.Common.ApiRequests.Auth;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;
    private readonly IRoleAssignmentService _roleService;
    private readonly IRolePermissionService _permissionService;
    private readonly IRepository<RolePermission> _rolePerRepo;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IAccessScopeResolver _scopeResolver;
    private readonly TrainingDbContext _training;

    public AuthService(AppDbContext db, TrainingDbContext training, IJwtService jwt, IEmailService email, IRepository<RolePermission> rolePerRepo, IRoleAssignmentService roleService, IRolePermissionService permissionService, IBlobStorageService blobStorageService, IAccessScopeResolver scopeResolver)
    {
        _db = db;
        _training = training;
        _jwt = jwt;
        _email = email;
        _rolePerRepo = rolePerRepo;
        _roleService = roleService;
        _permissionService = permissionService;
        _blobStorageService = blobStorageService;
        _scopeResolver = scopeResolver;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var (user, db, schema) = await FindEmployeeAsync(request.Email);

        if (user == null)
            throw new AppException(
                ErrorCodes.Invalid,
                StatusCodes.Status400BadRequest);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException(
                 ErrorCodes.Invalid,
                StatusCodes.Status400BadRequest);

        if (!user.IsActive || user.IsDeleted || (user.Company != null && !user.Company.IsActive))
            throw new AppException(
                ErrorCodes.EmployeeInactive,
                StatusCodes.Status403Forbidden);

        //string? profileImageWithoutSas = null;
        //try
        //{
        //    profileImageWithoutSas = await _db.Attachments
        //        .AsNoTracking()
        //        .Where(a =>
        //            a.ReferenceId == user.Id &&
        //            a.AttachmentType == AttachmentType.Employee &&
        //            !a.IsDeleted)
        //        .OrderByDescending(a => a.Id)
        //        .Select(a => a.FilePath)
        //        .FirstOrDefaultAsync();
        //}
        //catch (Exception)
        //{
        //    // Profile image must not block login (Attachments can be large / unindexed under load).
        //    profileImageWithoutSas = null;
        //}

        //var profileImage = _blobStorageService.WithSas(profileImageWithoutSas);



        var userRoles = await db.EmployeeRoles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(er =>
                er.EmployeeId == user.Id &&
                er.IsAssigned &&
                !er.IsDeleted &&
                er.Role != null &&
                !er.Role.IsDeleted)
            .Select(er => new { er.Role.Name, er.Role.Level })
            .ToListAsync();

        var roles = userRoles
            .Select(r => r.Name)
            .Distinct()
            .ToList();

        int roleLevel = userRoles.Any()
            ? userRoles.Max(r => r.Level)
            : (int)RoleLevelEnum.Employee;

        string roleLevelName =
            Enum.GetName(typeof(RoleLevelEnum), roleLevel) ?? "Employee";

        var permissions = await db.EmployeeRoles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(er =>
                er.EmployeeId == user.Id &&
                er.IsAssigned &&
                !er.IsDeleted &&
                er.Role != null &&
                !er.Role.IsDeleted)
            .SelectMany(er => er.Role.RolePermissions)
            .Where(rp =>
                rp.IsAssigned &&
                !rp.IsDeleted &&
                rp.Permission != null &&
                !rp.Permission.IsDeleted)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();

        // Same gate as AccessScopeResolver: no role / SelfOnly must not unlock نطاق المهام in the UI.
        var scope = await _scopeResolver.ResolveAsync(user.Id);
        var hasAccessScope =
            scope.Kind is AccessScopeKind.ManagerScoped or AccessScopeKind.CompanyWide
            || permissions.Any(p =>
                string.Equals(p, PermissionCodes.ViewCompanyTasks, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p, PermissionCodes.ViewScopedTasks, StringComparison.OrdinalIgnoreCase));

        var token = await _jwt.GenerateTokenAsync(user, schema);

        user.LastLoginDate = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CompanyId = user.CompanyId,
            campanyName = user.Company?.Name,
            BranchId = user.BranchId,
            branchName = user.Branch?.Name,
            DepartmentId = user.DepartmentId,
            deptName = user.Department?.Name,
            JobId = user.JobId,
            Title = user.Title,
            Nationality = user.Nationality,
            IdentityNumber = user.IdentityNumber,
            Mobile = user.Mobile,
            Address = user.Address,
            Qualification = user.Qualification,
            IsActive = user.IsActive,
            //ProfileImage = profileImage,
            Roles = roles,
            Permissions = permissions,
            RoleLevel = roleLevel,
            RoleLevelName = roleLevelName,
            EmployeeTypeId = user.EmployeeTypeId,
            HasAccessScope = hasAccessScope,
            Schema = schema,

            Token = token
        });
    }

    private async Task<(Employee? User, AppDbContext Db, string Schema)> FindEmployeeAsync(string email)
    {
        var dboUser = await _db.Employees
            .Include(u => u.Company)
            .Include(u => u.Branch)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

        if (dboUser != null)
            return (dboUser, _db, OperationalSchema.Dbo);

        // Oldest first, so rows duplicated before emails were unique across trainees resolve the same way every time.
        var trainingUser = await _training.Employees
            .IgnoreQueryFilters()
            .Include(u => u.Company)
            .Include(u => u.Branch)
            .Include(u => u.Department)
            .Where(u => u.Email == email && !u.IsDeleted)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync();

        if (trainingUser != null)
            return (trainingUser, _training, OperationalSchema.Training);

        return (null, _db, OperationalSchema.Dbo);
    }


    public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
    {
        var (user, db, _) = await FindEmployeeAsync(email);

        if (user == null)
            throw new AppException(
                ErrorCodes.EmailNotFound,
                StatusCodes.Status404NotFound);

        if (!user.IsActive || user.IsDeleted)
            throw new AppException(
                ErrorCodes.EmployeeInactive,
                StatusCodes.Status403Forbidden);

        var random = new Random();
        string resetCode = random.Next(100000, 999999).ToString();

        user.ResetPasswordToken = BCrypt.Net.BCrypt.HashPassword(resetCode);
        user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(30);

        await db.SaveChangesAsync();

        await _email.SendEmailAsync(email, "Password Reset Code", $"Your reset code: {resetCode}");

        return ApiResponse<bool>.Ok(true, "Reset code sent to email");
    }


    public async Task<ApiResponse<bool>> VerifyResetCodeAsync(VerifyResetCodeRequest request)
    {
        var (user, _, _) = await FindEmployeeAsync(request.Email);
        if (user != null && user.ResetPasswordExpiry.GetValueOrDefault() <= DateTime.UtcNow)
            user = null;

        if (user == null)
            throw new AppException(
                ErrorCodes.EmailNotFound,
                StatusCodes.Status404NotFound);

        if (!user.IsActive || user.IsDeleted)
            throw new AppException(
                ErrorCodes.EmployeeInactive,
                StatusCodes.Status403Forbidden);

        bool isValidCode = BCrypt.Net.BCrypt.Verify(request.Token, user.ResetPasswordToken);
        if (!isValidCode)
            throw new AppException(ErrorCodes.InvalidToken, StatusCodes.Status400BadRequest);

        return ApiResponse<bool>.Ok(true, "Token is valid");
    }




    public async Task<ApiResponse<bool>> UpdatePasswordAsync(UpdatePasswordRequest request)
    {
        var (user, db, _) = await FindEmployeeAsync(request.Email);

        if (user == null)
            throw new AppException(
                ErrorCodes.EmailNotFound,
                StatusCodes.Status404NotFound);

        if (!user.IsActive || user.IsDeleted)
            throw new AppException(
                ErrorCodes.EmployeeInactive,
                StatusCodes.Status403Forbidden);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.ResetPasswordToken = string.Empty;
        user.ResetPasswordExpiry = null;

        await db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Password reset successfully");
    }



}
