using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Offer;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

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
            if (!result.Success) 
                return Fail(result.Message!, 404);

            return Success(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] OfferAddEditDto dto)
        {
            var result = await _service.AddAsync(dto);
            if (!result.Success) 
                return Fail(result.Message);

            return Success(result.Data, "Offer added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OfferAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result.Success) 
                return Fail(result.Message, 404);

            return Success(result.Data, "Offer updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success) 
                return Fail(result.Message, 404);

            return Success(true, "Offer deleted successfully");
        }
    }
}
