using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using TaskMangment.Application.Common.ApiRequests.Auth;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Interfaces.Services;  
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;

    public AuthService(AppDbContext db, IJwtService jwt, IEmailService email)
    {
        _db = db;
        _jwt = jwt;
        _email = email;
    }
     
    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _db.Employees.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
                throw new AppException(
                    ErrorCodes.EmailNotFound,
                    StatusCodes.Status404NotFound);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new AppException(
                    ErrorCodes.Invalid,
                    StatusCodes.Status404NotFound);
        var token = _jwt.GenerateToken(user);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                DepartmentId = user.DepartmentId,
                JobId = user.JobId,
                Title = user.Title,
                Nationality = user.Nationality,
                IdentityNumber = user.IdentityNumber,
                Mobile = user.Mobile,
                Address = user.Address,
                Qualification = user.Qualification,
                IsActive = user.IsActive,
                //Roles = roles,
                //Permissions = permissions,
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

        string resetToken = Guid.NewGuid().ToString();

        user.ResetPasswordToken = resetToken;
        user.ResetPasswordExpiry = DateTime.UtcNow.AddMinutes(30);

        await _db.SaveChangesAsync();

        await _email.SendEmailAsync(email, "Password Reset Code", $"Your reset code: {resetToken}");

        return ApiResponse<bool>.Ok(true, "Reset code sent to email");
    }
     
    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _db.Employees
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email &&
                u.ResetPasswordToken == request.Token &&
                u.ResetPasswordExpiry > DateTime.UtcNow);

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
