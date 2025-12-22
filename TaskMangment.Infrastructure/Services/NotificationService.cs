using Microsoft.AspNetCore.SignalR;
using TaskMangment.Infrastructure.SignalR;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;


public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly ISignalRNotifier _signalRNotifier;
    private readonly IEmailQueueService _emailQueueService;
    //private readonly IWhatsAppQueueService _whatsAppQueueService;

    public NotificationService(
        INotificationRepository repo,
        ISignalRNotifier signalRNotifier,
        IEmailQueueService emailQueueService//,
        //IWhatsAppQueueService whatsAppQueueService
        )
    {
        _repo = repo;
        _signalRNotifier = signalRNotifier;
        _emailQueueService = emailQueueService;
        //_whatsAppQueueService = whatsAppQueueService;
    }

    public async Task SendAsync(
        int userId,
        string message,
        bool sendEmail,
        bool sendWhatsApp)
    {
        // 1️⃣ Save notification
        await _repo.AddAsync(new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = false,
        });

        // 2️⃣ SignalR (instant)
        await _signalRNotifier.NotifyAsync(userId, message);

        // 3️⃣ Email (queued)
        if (sendEmail)
        {
            await _emailQueueService.QueueAsync(
                userId,
                message,
                templateKey: "TaskAssigned"
            );
        }

        //// 4️⃣ WhatsApp (queued)
        //if (sendWhatsApp)
        //{
        //    await _whatsAppQueueService.QueueAsync(userId, message);
        //}
    }

    public Task<List<Notification>> GetUnreadAsync(int userId)
        => _repo.GetUnreadAsync(userId);

    public Task MarkAsReadAsync(int notificationId)
        => _repo.MarkAsReadAsync(notificationId);

    public Task MarkAllAsReadAsync(int userId)
        => _repo.MarkAllAsReadAsync(userId);
}


