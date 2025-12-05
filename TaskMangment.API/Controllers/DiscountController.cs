using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.CalenderEvents;
using TaskMangment.Application.Common.ApiRequests.Discount;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class DiscountController : BaseController
    {
        private readonly IDiscountService _service;

        public DiscountController(IDiscountService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DiscountRequest request)
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
        public async Task<IActionResult> Add(int createdByEmployeeId, int TaskId, [FromBody] DiscountAddEditDto dto)
        {
            var result = await _service.AddAsync(createdByEmployeeId, TaskId, dto);

            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data, "Discount added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,int TaskId, [FromBody] DiscountAddEditDto dto, int ModifiedByEmployeeId)
        {
            var result = await _service.UpdateAsync(id,TaskId, dto,ModifiedByEmployeeId);

            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(result.Data, "Discount updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(true, "Discount deleted successfully");
        }
    }

}
