using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Repositories;
using TaskMangment.Infrastructure.Services;
using TaskMangment.Infrastructure.SignalR;
using TaskMangment.Utilities.Localization.Resources;


public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IEmailQueueService _emailQueueService;
    private readonly INotificationSender _notificationSender;
    private readonly IOnlineUserService _onlineUserService;
    private readonly IStringLocalizer<TaskNotification> _L;
    private readonly IRepository<Employee> _EmployeeRepo;
    private readonly IWhatsAppService _whatsAppService;


    public NotificationService(
        INotificationRepository repo,
        IEmailQueueService emailQueueService, INotificationSender notificationSender, IOnlineUserService onlineUserService, IStringLocalizer<TaskNotification> localizer,
        IWhatsAppService whatsAppService,
        IRepository<Employee> EmployeeRepo)


    {
        _repo = repo;
        _emailQueueService = emailQueueService;
        _notificationSender = notificationSender;
        _onlineUserService = onlineUserService;
        _L = localizer;
        _EmployeeRepo = EmployeeRepo;
        _whatsAppService = whatsAppService;

    }

    public async Task SendAsync(int userId, string messageKey, bool sendEmail, bool sendWhatsApp, int? taskId, NotificationType type, int referenceId)
    {
        var message = messageKey;

        bool isOnline = _onlineUserService.IsUserOnline(userId);

        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            NotificationType = type,
            ReferenceId = referenceId,
            IsRead = isOnline,
            TaskId = taskId

        };

        await _repo.AddAsync(notification);

        if (isOnline)
        {
            await _notificationSender.SendWebAsync(userId, message, taskId);
            return;
        }
        if (sendWhatsApp)
        {
            var user = await _EmployeeRepo.GetByIDAsync(userId);

            if (user != null && !string.IsNullOrEmpty(user.Mobile))
            {
                await _whatsAppService.SendTaskAssignedNotification(
                    user.Mobile,
                    user.FullName,
                    message
                );
            }
        }
    }

    public Task<List<Notification>> GetUnreadAsync(int userId)
        => _repo.GetUnreadAsync(userId);

    public Task MarkAsReadAsync(int notificationId)
        => _repo.MarkAsReadAsync(notificationId);

    public Task MarkAllAsReadAsync(int userId)
        => _repo.MarkAllAsReadAsync(userId);


}


