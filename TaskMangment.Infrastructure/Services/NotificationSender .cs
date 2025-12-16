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
        private readonly IEmailService _email;
        private readonly IWhatsAppService _whatsapp;

        public NotificationSender(
            IHubContext<NotificationHub> hub,
            IEmailService email,
            IWhatsAppService whatsapp)
        {
            _hub = hub;
            _email = email;
            _whatsapp = whatsapp;
        }

        public async Task SendWebAsync(int userId, string message)
        {
            if (!NotificationHub._onlineUsers.TryGetValue(userId, out var connections))
                return;

            foreach (var connectionId in connections)
            {
                if (string.IsNullOrWhiteSpace(connectionId))
                    continue;
                await _hub.Clients
                    .Client(connectionId)
                    .SendAsync("ReceiveNotification", message);
            }
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            await _email.SendEmailAsync(email, subject, message);
        }

        public async Task SendWhatsAppAsync(string phone, string message)
        {
            await _whatsapp.SendMessageAsync(phone, message);
        }
    }

}
