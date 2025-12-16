using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskWarningController : BaseController
    {
        private readonly ITaskWarningService _service;

        public TaskWarningController(ITaskWarningService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] WarningRequest request)
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
        public async Task<IActionResult> Add(int tasktId, [FromBody] WarningAddEditDto dto)
        {
            var result = await _service.AddAsync(dto, tasktId,this.CurrentUserId);
            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data, "Warning added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarningAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(result.Data, "Warning updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(true, "Warning deleted successfully");
        }
    }
}

