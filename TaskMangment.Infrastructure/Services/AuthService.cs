using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Linq;
using TaskMangment.Application.Common.ApiRequests.Auth;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
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

    public AuthService(AppDbContext db, IJwtService jwt, IEmailService email, IRepository<RolePermission> rolePerRepo, IRoleAssignmentService roleService, IRolePermissionService permissionService, IBlobStorageService blobStorageService)
    {
        _db = db;
        _jwt = jwt;
        _email = email;
        _rolePerRepo = rolePerRepo;
        _roleService = roleService;
        _permissionService = permissionService;
        _blobStorageService = blobStorageService;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _db.Employees
    .Include(u => u.Company)
    .Include(u => u.Branch)
    .Include(u => u.Department)
    .FirstOrDefaultAsync(u => u.Email == request.Email);


        if (user == null)
            throw new AppException(
                ErrorCodes.Invalid,
                StatusCodes.Status400BadRequest);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException(
                 ErrorCodes.Invalid,
                StatusCodes.Status400BadRequest);

        var profileImageWithoutSas = await _db.Attachments
            .Where(a =>
                a.ReferenceId == user.Id &&
                a.AttachmentType == AttachmentType.Employee &&
                !a.IsDeleted)
            .Select(a => a.FilePath)
            .FirstOrDefaultAsync();
        var profileImage = _blobStorageService.WithSas(profileImageWithoutSas);



        var userRoles = await _roleService.GetUserRolesAsync(user.Id);

        var roles = userRoles
            .Select(r => r.Name)
            .Distinct()
            .ToList();

        int roleLevel = userRoles.Any()
            ? userRoles.Max(r => r.Level)
            : (int)RoleLevelEnum.Employee;

        string roleLevelName =
            Enum.GetName(typeof(RoleLevelEnum), roleLevel) ?? "Employee";

        var permissions = await _permissionService
                            .GetUserPermissionsAsync(user.Id);

        var token = await _jwt.GenerateTokenAsync(user);

        user.LastLoginDate = DateTime.UtcNow;
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
            ProfileImage = profileImage,
            Roles = roles,
            Permissions = permissions,
            RoleLevel = roleLevel,
            RoleLevelName = roleLevelName,
            FunctionCode= user.FunctionCode,

            Token = token
        });
    }


    public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
    {
        var user = await _db.Employees.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            throw new AppException(
                ErrorCodes.EmailNotFound,
                StatusCodes.Status404NotFound);

        var random = new Random();
        string resetCode = random.Next(100000, 999999).ToString();

        user.ResetPasswordToken = BCrypt.Net.BCrypt.HashPassword(resetCode);
        user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(30);

        await _db.SaveChangesAsync();

        await _email.SendEmailAsync(email, "Password Reset Code", $"Your reset code: {resetCode}");

        return ApiResponse<bool>.Ok(true, "Reset code sent to email");
    }


    public async Task<ApiResponse<bool>> VerifyResetCodeAsync(VerifyResetCodeRequest request)
    {
        var user = await _db.Employees
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email &&
                u.ResetPasswordExpiry > DateTime.UtcNow);

        if (user == null)
            throw new AppException(
                ErrorCodes.EmailNotFound,
                StatusCodes.Status404NotFound);

        bool isValidCode = BCrypt.Net.BCrypt.Verify(request.Token, user.ResetPasswordToken);
        if (!isValidCode)
            throw new AppException(ErrorCodes.InvalidToken, StatusCodes.Status400BadRequest);

        return ApiResponse<bool>.Ok(true, "Token is valid");
    }




    public async Task<ApiResponse<bool>> UpdatePasswordAsync(UpdatePasswordRequest request)
    {
        var user = await _db.Employees
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            throw new AppException(
                ErrorCodes.EmailNotFound,
                StatusCodes.Status404NotFound);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.ResetPasswordToken = string.Empty;
        user.ResetPasswordExpiry = null;

        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Password reset successfully");
    }



}
