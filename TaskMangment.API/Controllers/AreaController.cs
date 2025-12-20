using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.ApiRequests.Area;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class AreaController : BaseController
    {
        private readonly IAreaService _service;

        public AreaController(IAreaService service)
        {
            _service = service;
        }
  

    [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AreaRequest request)
        {
            var result = await _service.GetAllAsync(request, this.CompanyId); 
            //SetCacheHeader(600);
            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id); 
            return Success(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AreaAddEditDto dto )
        {
            var result = await _service.AddAsync(dto, this.CompanyId, this.CurrentUserId); 
            return Success(result.Data, "Area added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AreaAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Area updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id); 
            return Success(true, "Area deleted successfully");
        }
    }

}
