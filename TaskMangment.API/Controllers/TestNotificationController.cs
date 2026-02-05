using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestNotificationController : ControllerBase
    {
        private readonly INotificationService _notification;
        private readonly IHubContext<NotificationHub> _hub;

        public TestNotificationController(INotificationService notification,IHubContext<NotificationHub> hub)
        {
            _notification = notification;
            _hub = hub;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendTest()
        {
            int testUserId = 1;

            await _hub.Clients.All.SendAsync("ReceiveLog", " Hello from Task Mangment!");

            await _notification.SendAsync(
                userId: testUserId,
                message: " Test Notification from API!",
                sendEmail: true,
                sendWhatsApp: true,
                taskId: null,
                NotificationType.Comments,
                0

            );

            return Ok("Notification Sent!");
        }
    }
}
