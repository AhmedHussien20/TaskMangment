using Microsoft.Extensions.Localization; 
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors
{
    public class TaskWarningEventHandler : IEventHandler<TaskWarningEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer _localizer;

        public TaskWarningEventHandler(
            INotificationService notificationService,
            IStringLocalizerFactory factory)
        {
            _notificationService = notificationService;

            _localizer = factory.Create("TaskNotification", "TaskMangment.API");
        }

        public async Task Handle(TaskWarningEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.TaskWarningNotification
            ];

            var message = string.Format(
                messageTemplate,
                ev.TaskTitle,
                ev.IssuedbyName
            );


            await _notificationService.SendAsync(
                ev.IssuedtoId,
                message,
                sendEmail: true,
                sendWhatsApp: false
            );

        }
    }
}
