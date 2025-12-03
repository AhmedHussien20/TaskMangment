using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Job;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class JobController : BaseController
    {
        private readonly IJobService _service;

        public JobController(IJobService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] JobRequest request)
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
        public async Task<IActionResult> Add([FromBody] JobAddEditDto dto)
        {
            var result = await _service.AddAsync(dto);

            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Job added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JobAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(true, "Job updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result.Success)
                return Fail(result.Message, 404);

            return Success(true, "Job deleted successfully");
        }
    }
}
