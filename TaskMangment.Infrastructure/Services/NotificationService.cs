using Microsoft.AspNetCore.SignalR;
using TaskMangment.Infrastructure.SignalR;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;


public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IEmailQueueService _emailQueueService;
    private readonly INotificationSender _notificationSender;
    private readonly IOnlineUserService _onlineUserService;

    public NotificationService(
        INotificationRepository repo,
        IEmailQueueService emailQueueService,
        INotificationSender notificationSender,
        IOnlineUserService onlineUserService)
    {
        _repo = repo;
        _emailQueueService = emailQueueService;
        _notificationSender = notificationSender;
        _onlineUserService = onlineUserService;
    }

    public async Task SendAsync(int userId,string message, bool sendEmail,bool sendWhatsApp)
    {
        bool isOnline = _onlineUserService.IsUserOnline(userId);

        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = isOnline
        };

        await _repo.AddAsync(notification);

        if (isOnline)
        {
            await _notificationSender.SendWebAsync(userId, message);
            return;
        }
    }

    public Task<List<Notification>> GetUnreadAsync(int userId)
        => _repo.GetUnreadAsync(userId);

    public Task MarkAsReadAsync(int notificationId)
        => _repo.MarkAsReadAsync(notificationId);

    public Task MarkAllAsReadAsync(int userId)
        => _repo.MarkAllAsReadAsync(userId);
}


