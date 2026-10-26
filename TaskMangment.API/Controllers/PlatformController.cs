using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.ApiRequests.Platform;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/platform")]
    [ApiController]
    public class PlatformController : BaseController
    {
        private readonly IPlatformService _platform;

        public PlatformController(IPlatformService platform)
        {
            _platform = platform;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PlatformLoginRequest request)
        {
            var result = await _platform.LoginAsync(request);
            return Success(result.Data);
        }

        [HttpGet("organizations")]
        public async Task<IActionResult> Organizations([FromQuery] OrganizationListRequest request)
        {
            var result = await _platform.GetOrganizationsAsync(CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpPost("organizations")]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
        {
            var result = await _platform.CreateOrganizationAsync(CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpGet("trainees")]
        public async Task<IActionResult> Trainees([FromQuery] TraineeCopyListRequest request)
        {
            var result = await _platform.GetTraineesAsync(CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpPost("trainees")]
        public async Task<IActionResult> CreateTrainee([FromBody] CreateTraineeRequest request)
        {
            var result = await _platform.CreateTraineeAsync(CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpPost("trainees/{companyId:int}/active")]
        public async Task<IActionResult> SetTraineeActive(int companyId, [FromBody] SetTraineeActiveRequest request)
        {
            var result = await _platform.SetTraineeActiveAsync(CurrentUserId, companyId, request.IsActive);
            return Success(result.Data);
        }

        [HttpGet("admins")]
        public async Task<IActionResult> Admins([FromQuery] PlatformAdminListRequest request)
        {
            var result = await _platform.GetPlatformAdminsAsync(CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpPost("admins")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreatePlatformAdminRequest request)
        {
            var result = await _platform.CreatePlatformAdminAsync(CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpPost("admins/{adminId:int}/active")]
        public async Task<IActionResult> SetAdminActive(int adminId, [FromBody] SetTraineeActiveRequest request)
        {
            var result = await _platform.SetPlatformAdminActiveAsync(CurrentUserId, adminId, request.IsActive);
            return Success(result.Data);
        }
    }
}
