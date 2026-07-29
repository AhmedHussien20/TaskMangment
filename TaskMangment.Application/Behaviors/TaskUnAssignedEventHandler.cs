using Microsoft.Extensions.Localization;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Application.Behaviors
{
    public class TaskUnAssignedEventHandler : IEventHandler<TaskUnAssignedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;

        public TaskUnAssignedEventHandler(
            INotificationService notificationService,
            IStringLocalizer<TaskNotification> localizer)
        {
            _notificationService = notificationService;
            _localizer = localizer;
        }

        public async Task Handle(TaskUnAssignedEvent ev)
        {
            var messageTemplate = _localizer[NotificationCode.TaskUnAssignedNotification];
            var subjectName = string.IsNullOrWhiteSpace(ev.SubjectEmployeeName) ? "-" : ev.SubjectEmployeeName;
            var message = string.Format(messageTemplate, ev.TaskId, ev.TaskTitle, subjectName);

            foreach (var empId in ev.RecipientIds)
            {
                await _notificationService.SendAsync(
                    empId,
                    message,
                    sendEmail: true,
                    sendWhatsApp: true,
                    ev.TaskId,
                    NotificationType.TaskUnassign,
                    ev.TaskId
                );
            }
        }
    }
}
