using Microsoft.AspNetCore.Mvc; 
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskAssignmentController : ControllerBase
    {
        private readonly ITaskAssignmentService _service;

        public TaskAssignmentController(ITaskAssignmentService service)
        {
            _service = service;
        }

        [HttpPost("filter")]
        public async Task<IActionResult> GetAll([FromBody] TaskAssignmentRequest request)
        {
            return Ok(await _service.GetAllAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TaskAssignmentAddEditDto dto)
        {
            return Ok(await _service.AddAsync(dto));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskAssignmentAddEditDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _service.DeleteAsync(id));
        }
    }
}
