using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class TaskExtensionRequestController : BaseController
    {
        private readonly ITaskExtensionRequestsService _service;

        public TaskExtensionRequestController(ITaskExtensionRequestsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskExtensionRequestRequest request)
        {
            var result = await _service.GetAllAsync(request);

            if (!result.Success)
                return Fail(result.Message!);

            SetCacheHeader(600);

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (!result.Success)
                return Fail(result.Message!, 404);

            return Success(result.Data);
        }

        [HttpPost("{taskId}")]
        public async Task<IActionResult> Add([FromBody] TaskExtensionRequestAddDto dto, int taskId)
        {
            var result = await _service.AddAsync(dto, taskId, this.CurrentUserId);

            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data, "Extension request added successfully");
        }

        [HttpPut("{id}/review")]
        public async Task<IActionResult> Review(int id, [FromQuery] bool approved)
        {
            var result = await _service.ReviewAsync(id, approved, this.CurrentUserId);

            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data, "Extension request reviewed successfully");
        }
    }
}
