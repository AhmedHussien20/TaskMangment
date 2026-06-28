using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class NotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hub; 

        public NotificationSender(IHubContext<NotificationHub> hub  )
        {
            _hub = hub; 
        }

        public async Task SendWebAsync(int userId, string message, int? taskId, int notificationId)
        {
            if (!NotificationHub._onlineUsers.TryGetValue(userId, out var connections))
                return;

            foreach (var connectionId in connections)
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    continue;
                await _hub.Clients .Client(connectionId) .SendAsync("ReceiveNotification", new { id = notificationId, message, taskId });
                   
            }
        } 

    }

}
