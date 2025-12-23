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
    public class SignalRNotifier : ISignalRNotifier
    {
        private readonly IHubContext<NotificationHub> _hub;

        public SignalRNotifier(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task NotifyAsync(int userId, string message)
        {
            await _hub.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", message);
        }
    }
}
