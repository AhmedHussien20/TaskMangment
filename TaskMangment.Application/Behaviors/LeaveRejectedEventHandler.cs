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
    public class LeaveRejectedEventHandler : IEventHandler<LeaveRejectedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;
        private readonly IGetHigherManager _getHigherManager;

        public LeaveRejectedEventHandler(
            INotificationService notificationService,
           IStringLocalizer<TaskNotification> localizer,
           IGetHigherManager getHigherManager)
        {
            _notificationService = notificationService;
            _localizer = localizer;
            _getHigherManager = getHigherManager;

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

            if (ev.EmployeeId != ev.RejectedById)
            {
                await _notificationService.SendAsync(
                    ev.EmployeeId,
                    message,
                    sendEmail: false,
                    sendWhatsApp: true,
                    null,
                    NotificationType.leaverejected,
                    ev.LeaveId
                );
            }

            var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(ev.EmployeeId);
            foreach (var managerId in managerIds.Where(id => id != ev.EmployeeId && id != ev.RejectedById))
            {
                await _notificationService.SendAsync(
                    managerId,
                    message,
                    sendEmail: false,
                    sendWhatsApp: true,
                    null,
                    NotificationType.leaverejected,
                    ev.LeaveId
                );
            }
        }
    }

}
