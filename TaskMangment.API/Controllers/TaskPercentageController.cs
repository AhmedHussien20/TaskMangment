using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class TaskPercentageController : BaseController
    {
        private readonly ITaskPercentageService _service;

        public TaskPercentageController(ITaskPercentageService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskPercentRequest request)
        {
            var result = await _service.GetAllAsync(request);
            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost("{taskId}")]
        public async Task<IActionResult> Add(int taskId, [FromBody] TaskPercentageAddEditDto dto)
        {
            var result = await _service.AddAsync(taskId, this.CurrentUserId,this.Role, dto);
            return Success(result.Data, "Percentage added");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskPercentageAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, this.CurrentUserId, dto);
            return Success(result.Data, "Percentage updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true);
        }
    }
}
