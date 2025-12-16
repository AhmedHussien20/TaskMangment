using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Interfaces.IRepository;
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

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);

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
