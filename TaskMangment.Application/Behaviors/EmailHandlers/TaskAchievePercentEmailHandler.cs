using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors.EmailHandlers
{
    public class TaskAchievePercentEmailHandler : IEventHandler<TaskAchievePercentEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public TaskAchievePercentEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(TaskAchievePercentEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "TaskAchievement",
                ReferenceType = ReferenceType.TaskAchieve,
                RecipientType = RecipientType.Employee,
                ReferenceId = ev.PercentId,
                UserIds = ev.Recipients
            });
        }
    }
}
