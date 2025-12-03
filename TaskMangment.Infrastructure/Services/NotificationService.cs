using Microsoft.AspNetCore.SignalR;
using TaskMangment.Infrastructure.SignalR;
using TaskMangment.Application.Interfaces.Services;

public class NotificationService : INotificationService
{
    private readonly IWhatsAppService _whatsapp;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IEmailService _email;

    public NotificationService(
        IWhatsAppService whatsapp,
        IHubContext<NotificationHub> hub,
        IEmailService email)
    {
        _whatsapp = whatsapp;
        _hub = hub;
        _email = email;
    }

    public async Task SendAsync(int userId, string message, bool sendEmail, bool sendWhatsApp)
    {
        // 1) Web real-time
        await _hub.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", message);

        // 2) Email
        if (sendEmail)
            await _email.SendEmailAsync("user@mail.com", "Notification", message);

        // 3) WhatsApp
        if (sendWhatsApp)
            await _whatsapp.SendMessageAsync("01000000000", message);
    }
}
