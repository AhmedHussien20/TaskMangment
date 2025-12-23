using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors
{
    public class TaskCloseRequestEventHandler : IEventHandler<TaskCloseRequestEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer _localizer;

        public TaskCloseRequestEventHandler(
            INotificationService notificationService,
            IStringLocalizerFactory factory)
        {
            _notificationService = notificationService;

            _localizer = factory.Create("TaskNotification", "TaskMangment.API");
        }

        public async Task Handle(TaskCloseRequestEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.TaskCloseRequestNotification
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
