using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.Services;  
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Application.Common.ApiRequests.Auth;
using TaskMangment.Application.Responses;

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
            return ApiResponse<LoginResponse>.Fail("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<LoginResponse>.Fail("Invalid email or password");

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
           // Roles = roles,
            //Permissions = permissions,
            Token = token
        });
    } 

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
    {
        var user = await _db.Employees.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return ApiResponse<bool>.Fail("User not found");

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
            return ApiResponse<bool>.Fail("Invalid or expired token");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordExpiry = null;

        await _db.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Password reset successfully");
    }
}
