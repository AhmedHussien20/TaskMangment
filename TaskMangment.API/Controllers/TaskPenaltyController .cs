using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.CalenderEvents;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class TaskPenaltyController  : BaseController
    {
        private readonly ITaskDiscountService _service;

        public TaskPenaltyController (ITaskDiscountService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskDiscountRequest request)
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
        public async Task<IActionResult> Add(int taskId, [FromBody] DiscountAddEditDto dto)
        {
            var result = await _service.AddAsync(this.CurrentUserId, taskId, dto);
            return Success(result.Data, "Discount added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,int TaskId, [FromBody] DiscountAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id,TaskId, dto,this.CurrentUserId);
            return Success(result.Data, "Discount updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Discount deleted successfully");
        }
    }

}
