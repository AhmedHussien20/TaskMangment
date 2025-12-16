using Microsoft.AspNetCore.SignalR;
using TaskMangment.Infrastructure.SignalR;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly INotificationSender _sender;

    public NotificationService(
        INotificationRepository repo,
        INotificationSender sender)
    {
        _repo = repo;
        _sender = sender;
    }

    public async Task<List<Notification>> GetUnreadAsync(int userId)
    {
        return await _repo.GetUnreadAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _repo.MarkAsReadAsync(notificationId);
         
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _repo.MarkAllAsReadAsync(userId);
 
    }

    public async Task SendAsync(
        int userId,
        string message,
        bool sendEmail,
        bool sendWhatsApp)
    { 
        await _repo.AddAsync(new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = false,
        });
         
        await _sender.SendWebAsync(userId, message);
         
        if (sendEmail)
            await _sender.SendEmailAsync("user@mail.com", "Notification", message);
         
        if (sendWhatsApp)
            await _sender.SendWhatsAppAsync("01000000000", message);
    }
}

