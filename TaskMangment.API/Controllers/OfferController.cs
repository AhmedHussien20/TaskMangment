using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Offer;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class OfferController : BaseController
    {
        private readonly IOfferService _service;

        public OfferController(IOfferService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] OfferRequest request)
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
            return Success(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] OfferAddEditDto dto)
        {
            var result = await _service.AddAsync(dto);
            return Success(result.Data, "Offer added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OfferAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Offer updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Offer deleted successfully");
        }

        [HttpPost("{offerId}/assign-students")]
        public async Task<IActionResult> AssignStudents(int offerId,[FromBody] OfferAssignStudentsDto dto)
        {
            var result = await _service.AssignOfferToStudentsAsync(offerId, dto);
            return Success(true, "Offer assigned to students successfully");
        }
    }
}
