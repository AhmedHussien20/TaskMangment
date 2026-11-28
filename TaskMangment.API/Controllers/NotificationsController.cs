using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var notifications =
                await _notificationService.GetUnreadAsync(CurrentUserId);
            return Ok(ApiResponse<List<Notification>>.Ok(notifications));
        }

        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetByTask(int taskId)
        {
            var notifications =
                await _notificationService.GetByTaskAsync(CurrentUserId, taskId);
            return Ok(ApiResponse<List<TaskNotificationDto>>.Ok(notifications));
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);

            return Ok(ApiResponse<string>.Ok(string.Empty));
        }

        [HttpPost("by-task/{taskId}/read")]
        public async Task<IActionResult> MarkAsReadByTask(int taskId)
        {
            await _notificationService.MarkAsReadByTaskAsync(CurrentUserId, taskId);
            return Ok(ApiResponse<string>.Ok(string.Empty));
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(CurrentUserId);

            return Ok(ApiResponse<string>.Ok(string.Empty));
        }
    }

}
