using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskMangment.Application.Common.ApiRequests.Area; 
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly IRepository<Employee> _employeeRepo;
    private readonly IConfiguration _config;

    public AuthService(IRepository<Employee> employeeRepo, IConfiguration config)
    {
        _employeeRepo = employeeRepo;
        _config = config;
    }

    public async Task<ApiResponse<string>> LoginAsync(LoginRequest request)
    {
        var user = await _employeeRepo
            .GetAll(u => u.Email == request.Email)
            .FirstOrDefaultAsync();

        if (user == null)
            return ApiResponse<string>.Fail("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<string>.Fail("Invalid credentials");

        string token = JwtHelper.GenerateToken(
            user,
            _config["JWT:Key"],
            _config["JWT:Issuer"],
            _config["JWT:Audience"]
        );

        return ApiResponse<string>.Ok(token, "Login success");
    }
}
