using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class TaskCloseRequestController : BaseController
    {
        private readonly ITaskCloseRequestService _service;

        public TaskCloseRequestController(ITaskCloseRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskCloseRequestRequest request)
        {
            var result = await _service.GetAllAsync(request);

            if (!result.Success)
                return Fail(result.Message!);

            //SetCacheHeader(600);

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost("{taskId}")]
        [PermissionAuthorize(PermissionCodes.RequestTaskClose)]
        public async Task<IActionResult> Add([FromBody] TaskCloseRequestAddDto dto, int taskId)
        {
            var result = await _service.AddAsync(dto, taskId, this.CurrentUserId);
            return Success(result.Data, "Close request added successfully");
        }

        [HttpPatch("review/{id}")]
        [PermissionAuthorize(PermissionCodes.ApproveTaskRequest, PermissionCodes.RejectTaskRequest)]
        public async Task<IActionResult> Review(int id, [FromBody] CloseRequestStatus status)
        {
            var result = await _service.ReviewAsync(id, status, this.CurrentUserId);
            return Success(true, "Close request reviewed successfully");
        }
    }
}

