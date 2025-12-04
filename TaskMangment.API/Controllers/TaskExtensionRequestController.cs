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

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TaskExtensionRequestAddDto dto, int taskAssignmentId, int employeeId)
        {
            var result = await _service.AddAsync(dto, taskAssignmentId, employeeId);

            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Extension request added successfully");
        }

        [HttpPut("{id}/review")]
        public async Task<IActionResult> Review(int id, [FromQuery] bool approved, [FromQuery] int reviewerId)
        {
            var result = await _service.ReviewAsync(id, approved, reviewerId);

            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Extension request reviewed successfully");
        }
    }
}
