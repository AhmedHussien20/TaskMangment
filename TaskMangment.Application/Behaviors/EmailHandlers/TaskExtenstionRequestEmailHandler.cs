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
    public class TaskExtenstionRequestEmailHandler : IEventHandler<TaskExtensionRequestEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public TaskExtenstionRequestEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(TaskExtensionRequestEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "TaskExtensionRequest",
                ReferenceType = ReferenceType.TaskExtensionRequest,
                RecipientType = RecipientType.Employee,
                ReferenceId = ev.RequestId,
                UserIds = ev.Recipients
            });
        }
    }
}
