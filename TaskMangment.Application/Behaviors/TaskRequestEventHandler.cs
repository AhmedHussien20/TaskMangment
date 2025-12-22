using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors
{
    public class TaskRequestEventHandler : IEventHandler<TaskRequestAddedEvent>
    {
        private readonly INotificationService _notificationService;

        public TaskRequestEventHandler(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Handle(TaskRequestAddedEvent ev)
        {

            foreach (var empId in ev.Recipients)
            {
                await _notificationService.SendAsync(
                    empId,
                    $"A new requst on task: {ev.taskTitle} from {ev.EmployeeName}",

                    sendEmail: true,
                    sendWhatsApp: false
                );
            }
        }
    }
}
