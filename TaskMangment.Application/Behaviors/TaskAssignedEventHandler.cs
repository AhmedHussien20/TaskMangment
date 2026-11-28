using Microsoft.Extensions.Localization;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Application.Behaviors
{
    public class TaskAssignedEventHandler : IEventHandler<TaskAssignedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<TaskNotification> _localizer;

        public TaskAssignedEventHandler(
            INotificationService notificationService,
            IStringLocalizer<TaskNotification> localizer)
        {
            _notificationService = notificationService;
            _localizer = localizer;
        }

        public async Task Handle(TaskAssignedEvent ev)
        {
            var messageTemplate = _localizer[NotificationCode.TaskAssignedNotification];
            var subjectName = string.IsNullOrWhiteSpace(ev.SubjectEmployeeName) ? "-" : ev.SubjectEmployeeName;
            var message = string.Format(messageTemplate, ev.TaskId, ev.TaskTitle, subjectName, ev.BranchName);

            if (ev.CommentAllowPeriodDays.HasValue && ev.CommentAllowPeriodDays.Value > 0)
            {
                var requirementTemplate = _localizer["TASK_ASSIGNED_COMMENT_REQUIREMENT"];
                message = $"{message}\n{string.Format(requirementTemplate, ev.MinCommentsPerPeriod, ev.CommentAllowPeriodDays.Value)}";
            }

            foreach (var empId in ev.RecipientIds)
            {
                await _notificationService.SendAsync(
                    empId,
                    message,
                    sendEmail: true,
                    sendWhatsApp: true,
                    ev.TaskId,
                    NotificationType.TaskAssign,
                    ev.TaskId
                );
            }
        }
    }
}
