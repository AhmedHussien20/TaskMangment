using Microsoft.Extensions.Localization; 
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Application.Behaviors
{
    public class TaskWarningEventHandler : IEventHandler<TaskWarningEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;

        public TaskWarningEventHandler(
            INotificationService notificationService,
            IStringLocalizer<TaskNotification> localizer)
        {
            _notificationService = notificationService;
            _localizer = localizer;
            //_localizer = factory.Create("TaskNotification", "TaskMangment.API");
        }

        public async Task Handle(TaskWarningEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.TaskWarningNotification
            ];

            var message = string.Format(
                messageTemplate,
                ev.TaskId,
                ev.TaskTitle,
                ev.IssuedbyName,
                string.IsNullOrWhiteSpace(ev.IssuedToName) ? "-" : ev.IssuedToName
            );

            foreach (var empId in ev.SendTo)
            {
                await _notificationService.SendAsync(
                empId,
                message,
                sendEmail: true,
                sendWhatsApp: true,
                ev.TaskId,
                NotificationType.Warning,
                ev.WarningId
            );
            }

        }
    }
}
