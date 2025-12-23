using Microsoft.Extensions.Localization;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors
{
    public class TaskExtenstionRequestEventHandler : IEventHandler<TaskExtensionRequestEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer _localizer;

        public TaskExtenstionRequestEventHandler(
            INotificationService notificationService,
            IStringLocalizerFactory factory)
        {
            _notificationService = notificationService;

            _localizer = factory.Create("TaskNotification", "TaskMangment.API");
        }

        public async Task Handle(TaskExtensionRequestEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.TaskExtensionRequestNotification
            ];

            var message = string.Format(
                messageTemplate,
                ev.taskTitle,
                ev.EmployeeName
            );

            foreach (var empId in ev.Recipients)
            {
                await _notificationService.SendAsync(
                    empId,
                    message,
                    sendEmail: true,
                    sendWhatsApp: false
                );
            }
        }
    }
}
