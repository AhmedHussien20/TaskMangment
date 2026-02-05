using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //_localizer = factory.Create("TaskNotification", "TaskMangment.API");
        }

        public async Task Handle(TaskUnAssignedEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.TaskUnAssignedNotification
            ];

            var message = string.Format(
                messageTemplate,
                ev.TaskTitle
            );

            foreach (var empId in ev.UnAssignedEmployeeIds)
            {
                await _notificationService.SendAsync(
                    empId,
                    message,
                    sendEmail: true,
                    sendWhatsApp: false,
                    ev.TaskId,
                    NotificationType.TaskUnassign,
                    ev.TaskId
                );
            }
        }
    }
}
