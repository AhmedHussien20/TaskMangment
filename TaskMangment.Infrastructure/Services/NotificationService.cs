using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.Services;
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
    private readonly WhatsAppSettings _whatsAppSettings;
    private readonly ILogger<NotificationService> _logger;


    public NotificationService(
        INotificationRepository repo,
        IEmailQueueService emailQueueService, INotificationSender notificationSender, IOnlineUserService onlineUserService, IStringLocalizer<TaskNotification> localizer,
        IWhatsAppService whatsAppService,
        IOptions<WhatsAppSettings> whatsAppOptions,
        IRepository<Employee> EmployeeRepo,
        ILogger<NotificationService> logger)


    {
        _repo = repo;
        _emailQueueService = emailQueueService;
        _notificationSender = notificationSender;
        _onlineUserService = onlineUserService;
        _L = localizer;
        _EmployeeRepo = EmployeeRepo;
        _whatsAppService = whatsAppService;
        _whatsAppSettings = whatsAppOptions.Value;
        _logger = logger;

    }

    public async Task SendAsync(int userId, string messageKey, bool sendEmail, bool sendWhatsApp, int? taskId, NotificationType type, int referenceId, string? whatsAppMessage = null, IReadOnlyList<WhatsAppAttachment>? whatsAppAttachments = null)
    {
        var message = messageKey;
        var user = await _EmployeeRepo.GetByIDAsync(userId);
        if (user == null || !user.IsActive)
            return;

        bool isOnline = _onlineUserService.IsUserOnline(userId);

        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            NotificationType = type,
            ReferenceId = referenceId,
            IsRead = false,
            TaskId = taskId

        };

        await _repo.AddAsync(notification);

        if (isOnline)
        {
            try
            {
                await _notificationSender.SendWebAsync(userId, message, taskId, notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalR notification failed for user {UserId}", userId);
            }
        }
        if (sendWhatsApp)
        {
            try
            {
                var maxRoleLevel = await GetEmployeeMaxRoleLevelAsync(userId);
                if (maxRoleLevel < (int)RoleLevelEnum.Manager)
                {
                    _logger.LogInformation(
                        "Skipping WhatsApp for user {UserId}: role level {RoleLevel} is below Manager",
                        userId,
                        maxRoleLevel);
                }
                else if (!string.IsNullOrEmpty(user.Mobile))
                {
                    var whatsAppBody = BuildWhatsAppMessage(whatsAppMessage ?? message, taskId);
                    await _whatsAppService.SendNotificationAsync(
                        user.Mobile,
                        user.FullName,
                        whatsAppBody,
                        whatsAppAttachments
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp notification failed for user {UserId}", userId);
            }
        }
    }

    public Task<List<Notification>> GetUnreadAsync(int userId)
        => _repo.GetUnreadAsync(userId);

    public Task MarkAsReadAsync(int notificationId)
        => _repo.MarkAsReadAsync(notificationId);

    public Task MarkAllAsReadAsync(int userId)
        => _repo.MarkAllAsReadAsync(userId);

    private async Task<int> GetEmployeeMaxRoleLevelAsync(int employeeId)
    {
        var maxLevel = await _EmployeeRepo.GetAll(e => e.Id == employeeId)
            .SelectMany(e => e.EmployeeRoles)
            .Where(er => er.IsAssigned && !er.IsDeleted && er.Role != null)
            .Select(er => (int?)er.Role.Level)
            .MaxAsync();

        return maxLevel ?? (int)RoleLevelEnum.Employee;
    }

    private string BuildWhatsAppMessage(string message, int? taskId)
    {
        if (!taskId.HasValue || string.IsNullOrWhiteSpace(_whatsAppSettings.FrontendUrl))
            return message;

        var taskUrl = $"{_whatsAppSettings.FrontendUrl.TrimEnd('/')}/task/task-list?taskId={taskId.Value}";
        var taskLinkLabel = _L["TASK_LINK_LABEL"];
        return $"{message}\n\n{taskLinkLabel}:\n{taskUrl}";
    }


}

