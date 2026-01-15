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
        public async Task<IActionResult> Add([FromBody] TaskExtensionRequestAddDto dto, int taskId)
        {
            var result = await _service.AddAsync(dto, taskId, this.CurrentUserId);
            return Success(result.Data, "Extension request added successfully");
        }

        [HttpPatch("review/{id}")]
        public async Task<IActionResult> Review(int id, [FromBody] TaskExtensionReviewDto dto)
        {
            var result = await _service.ReviewAsync(id, dto, this.CurrentUserId);
            return Success(result.Data, "Extension request reviewed successfully");
        }
    }
}
