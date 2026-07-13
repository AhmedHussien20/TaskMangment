using Microsoft.Extensions.Localization; 
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Application.Behaviors
{
    public class LeaveApprovedEventHandler : IEventHandler<LeaveApprovedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;
        private readonly IGetHigherManager _getHigherManager;

        public LeaveApprovedEventHandler(
            INotificationService notificationService,
            IStringLocalizer<TaskNotification> localizer,
            IGetHigherManager getHigherManager)
        {
            _notificationService = notificationService;
            _localizer = localizer;
            _getHigherManager = getHigherManager;
            //_localizer = factory.Create("LeaveNotification", "TaskMangment.API");
        }

        public async Task Handle(LeaveApprovedEvent ev)
        {
            var template = _localizer[NotificationCode.LeaveApprovedNotification];

            var message = string.Format(
                template,
                ev.LeaveTypeName,
                ev.StartDate.ToString("yyyy-MM-dd"),
                ev.EndDate.ToString("yyyy-MM-dd"),
                ev.ApprovedByName
            );

            await _notificationService.SendAsync(
                ev.EmployeeId,
                message,
                sendEmail: false,
                sendWhatsApp: true,
                null,
                NotificationType.leaveApproved,
                ev.LeaveId
            );

            var managerId = await _getHigherManager.GetDirectHigherManagerIdAsync(ev.EmployeeId);
            if (managerId.HasValue && managerId.Value != ev.EmployeeId)
            {
                await _notificationService.SendAsync(
                    managerId.Value,
                    message,
                    sendEmail: false,
                    sendWhatsApp: true,
                    null,
                    NotificationType.leaveApproved,
                    ev.LeaveId
                );
            }
        }
    }

}
