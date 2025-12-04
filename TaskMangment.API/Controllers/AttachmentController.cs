using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Attachment;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class AttachmentController : BaseController
    {
        private readonly IAttachmentService _service;

        public AttachmentController(IAttachmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AttachmentRequest request)
        {
            var result = await _service.GetAllAsync(request);
            if (!result.Success) return Fail(result.Message!);
            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (!result.Success) return Fail(result.Message!);
            return Success(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] AttachmentAddDto dto)
        {
            var result = await _service.AddAsync(dto);
            if (!result.Success) return Fail(result.Message!);
            return Success(true, "Attachment added successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success) return Fail(result.Message!);
            return Success(true, "Attachment deleted successfully");
        }
    }
}
