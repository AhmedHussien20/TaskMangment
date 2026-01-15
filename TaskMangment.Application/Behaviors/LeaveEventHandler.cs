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
    public class LeaveEventHandler : IEventHandler<LeaveEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;

        public LeaveEventHandler(
            INotificationService notificationService,
            IStringLocalizer<TaskNotification> localizer)
        {
            _notificationService = notificationService;
            _localizer = localizer;
        }

        public async Task Handle(LeaveEvent ev)
        {
            var messageTemplate = _localizer[
                NotificationCode.LeaveRequestCreated
            ];

            var message = string.Format(
                messageTemplate,
                ev.LeaveTypeName,
                ev.EmployeeName,
                ev.StartDate.ToString("yyyy-MM-dd"),
                ev.EndDate.ToString("yyyy-MM-dd")
            );

            await _notificationService.SendAsync(
                userId: ev.EmployeeId,   
                message: message,
                sendEmail: false,
                sendWhatsApp: false
            );
        }
    }

}
