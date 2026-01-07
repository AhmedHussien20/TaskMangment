using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Authorization;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : BaseController
    {
        private readonly ILeaveService _service;

        public LeaveController(ILeaveService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(LeaveAddDto dto)
        {
            var result = await _service.CreateAsync(dto, this.CurrentUserId);
            return Success(result.Data);
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyRequests([FromQuery] BaseApiRequest request)
        {
            var result = await _service.GetMyRequestsAsync(this.CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpGet("pending")]
        [HasRole("Manager")]
        public async Task<IActionResult> Pending([FromQuery] BaseApiRequest request)
        {
            var result = await _service.GetPendingForApprovalAsync(this.CurrentUserId, request);
            return Success(result.Data);
        }

        [HttpPost("{id}/approve")]
        [HasRole("Manager")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _service.ApproveAsync(id, this.CurrentUserId);
            return Success(true);
        }

        [HttpPost("{id}/reject")]
        [HasRole("Manager")]
        public async Task<IActionResult> Reject(int id, [FromBody] string reason)
        {
            var result = await _service.RejectAsync(id, this.CurrentUserId, reason);
            return Success(true);
        }
    }



}
