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
    public class LeaveRejectedEventHandler : IEventHandler<LeaveRejectedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;

        public LeaveRejectedEventHandler(
            INotificationService notificationService,
           IStringLocalizer<TaskNotification> localizer)
        {
            _notificationService = notificationService;
            _localizer = localizer;

            //_localizer = factory.Create("LeaveNotification", "TaskMangment.API");
        }

        public async Task Handle(LeaveRejectedEvent ev)
        {
            var template = _localizer[NotificationCode.LeaveRejectedNotification];

            var message = string.Format(
                template,
                ev.LeaveTypeName,
                ev.StartDate.ToString("yyyy-MM-dd"),
                ev.EndDate.ToString("yyyy-MM-dd"),
                ev.RejectReason,
                ev.RejectedByName
            );

            await _notificationService.SendAsync(
                ev.EmployeeId,
                message,
                sendEmail: false,
                sendWhatsApp: false,
                null
            );
        }
    }

}
