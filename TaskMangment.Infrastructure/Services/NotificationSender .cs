using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class NotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ICurrentUserService _currentUser;

        public NotificationSender(IHubContext<NotificationHub> hub, ICurrentUserService currentUser)
        {
            _hub = hub;
            _currentUser = currentUser;
        }

        public async Task SendWebAsync(int userId, string message, int? taskId, int notificationId)
        {
            // The recipient lives in the same schema as the request (or Hangfire pass) that raised the notification.
            // A trainee can only reach colleagues in its own copy, so the sender's company is the recipient's.
            var key = NotificationHub.ConnectionKey(
                _currentUser.Schema ?? OperationalSchemaAmbient.Current, _currentUser.CompanyId, userId);

            foreach (var connectionId in NotificationHub.ConnectionsFor(key))
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    continue;
                await _hub.Clients .Client(connectionId) .SendAsync("ReceiveNotification", new { id = notificationId, message, taskId });

            }
        }

    }

}
