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
    public class TaskPenaltyEventHandler : IEventHandler<TaskPenaltyEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer _localizer;

        public TaskPenaltyEventHandler(
            INotificationService notificationService,
            IStringLocalizerFactory factory)
        {
            _notificationService = notificationService;

            _localizer = factory.Create("TaskNotification", typeof(TaskPenaltyEventHandler).Assembly.GetName().Name);
        }

        public async Task Handle(TaskPenaltyEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.TaskPenaltyNotification
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
