using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class TaskCommentController : BaseController
    {
        private readonly ITaskCommentService _service;

        public TaskCommentController(ITaskCommentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskCommentRequest request)
        {
            var result = await _service.GetAllAsync(request);
            if (!result.Success) return Fail(result.Message);
            //SetCacheHeader(300);
            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost("{taskId}")]
        [Consumes("multipart/form-data")]
        [PermissionAuthorize(PermissionCodes.CommentTask)]
        public async Task<IActionResult> Add(int taskId, [FromForm] TaskCommentAddEditDto dto)
        {
            var result = await _service.AddAsync(taskId, this.CurrentUserId,this.RoleLevel, dto);
            return Success(result.Data, "Comment added");
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(PermissionCodes.CommentTask)]
        public async Task<IActionResult> Update(int id, [FromBody] TaskCommentAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Comment updated");
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(PermissionCodes.CommentTask)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true);
        }

        [HttpGet("comments/{commentId}/attachments")]
        public async Task<IActionResult> GetCommentAttachments(int commentId)
        {
            var res = await _service.GetCommentAttachmentsAsync(commentId);
            return Ok(res);
        }

        [HttpGet("comments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            var file = await _service.DownloadAttachmentAsync(attachmentId);
            return File(file.Stream, file.ContentType, file.FileName);
        }


    }
}
