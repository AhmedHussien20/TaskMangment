using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<TGetDto, TAddEditDto> : ControllerBase
    {
        protected readonly dynamic _service;

        public BaseController(dynamic service)
        {
            _service = service;
        }

        [HttpGet]
        public virtual async Task<ActionResult<ApiResponse<ICollection<TGetDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return result;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<ApiResponse<TGetDto>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result;
        }

        [HttpPost]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Add(TAddEditDto dto)
        {
            var result = await _service.AddAsync(dto);
            return result;
        }

        [HttpPut("{id}")]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Update(int id, TAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return result;
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result;
        }
    }
}
