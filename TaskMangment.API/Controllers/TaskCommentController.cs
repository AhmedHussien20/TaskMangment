using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class TaskCommentController : BaseController
    {
        private readonly ITaskCommentService _service;

        public TaskCommentController(ITaskCommentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskCommentRequest request)
        {
            var result = await _service.GetAllAsync(request);
            if (!result.Success) return Fail(result.Message);
            SetCacheHeader(300);
            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost("{taskId}")]
        public async Task<IActionResult> Add(int taskId, [FromForm] TaskCommentAddEditDto dto)
        {
            var result = await _service.AddAsync(taskId, this.CurrentUserId, dto);
            return Success(result.Data, "Comment added");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskCommentAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Comment updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true);
        }

    }
}
