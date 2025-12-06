using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Common.ApiRequests.Auth;
using TaskMangment.API.Controllers;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        if (!result.Success)
            return Fail(result.Message);
        return Success(result.Data);
    }
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _auth.ForgotPasswordAsync(request.Email);
        if (!result.Success)
            return Fail(result.Message);
        return Success(result.Data);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _auth.ResetPasswordAsync(request);
        if(!result.Success)
            return Fail(result.Message);
        return Success(result.Data);
    }
}
