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
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Application.Behaviors
{
    public class TaskCommentEventHandler : IEventHandler<TaskCommentAddedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;

        public TaskCommentEventHandler(
            INotificationService notificationService,
             IStringLocalizer<TaskNotification> localizer)
        {
            _notificationService = notificationService;
            _localizer = localizer;
           // _localizer = factory.Create("TaskNotification", "TaskMangment.API");
        }

        public async Task Handle(TaskCommentAddedEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.TaskCommentNotification
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
                    sendWhatsApp: false,
                    ev.TaskId
                );
            }
        }
    }
}