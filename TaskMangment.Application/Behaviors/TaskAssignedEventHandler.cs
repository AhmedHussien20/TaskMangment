using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Notification;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors
{
    public class TaskAssignedEventHandler: IEventHandler<TaskAssignedEvent>
    {
        private readonly INotificationService _notificationService;

        public TaskAssignedEventHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Handle(TaskAssignedEvent ev)
        {
            foreach (var empId in ev.AssignedEmployeeIds)
            {
                await _notificationService.SendAsync(
                    empId,
                    $"A new task has been assigned to you: {ev.TaskTitle}",
                    sendEmail: true,
                    sendWhatsApp: false
                );
            }
        }
    }


}
