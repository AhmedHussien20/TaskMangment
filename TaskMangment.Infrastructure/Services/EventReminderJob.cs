using TaskMangment.Application.Common.ApiRequests.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class EventReminderJob
    {
        private readonly INotificationDispatcher _dispatcher;

        public async Task Execute(int userId, string message)
        {
            await _dispatcher.DispatchAsync(new NotificationCommand
            {
                UserId = userId,
                Message = message,
                Channels = new()
                        {
                            NotificationChannel.Web,
                            NotificationChannel.Email,
                            NotificationChannel.WhatsApp
                        }
            });
        }
    }

}
