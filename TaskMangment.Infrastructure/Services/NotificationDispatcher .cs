using Microsoft.AspNetCore.SignalR; 
using TaskMangment.Application.Common.ApiRequests.Notification;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IEmailService _email;
        private readonly IWhatsAppService _whatsApp;

        public async Task DispatchAsync(NotificationCommand command)
        {
            foreach (var channel in command.Channels)
            {
                await _repo.AddAsync(new Notification
                {
                    UserId = command.UserId,
                    Message = command.Message,
                    Channel = channel,
                });

                if (channel == NotificationChannel.Web &&
                    NotificationHub.IsUserOnline(command.UserId))
                {
                    await _hub.Clients
                        .Client(command.UserId.ToString())
                        .SendAsync("ReceiveNotification", command.Message);
                }

                if (channel == NotificationChannel.Email) { }
                    //await _email.SendAsync(command.UserId, command.Message);

                if (channel == NotificationChannel.WhatsApp) { }
                    //await _whatsApp.SendAsync(command.UserId, command.Message);
            }
        }
    }

}
