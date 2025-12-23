using Microsoft.Extensions.Localization;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors
{
    public class TaskRequestEmailHandler : IEventHandler<TaskRequestAddedEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public TaskRequestEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(TaskRequestAddedEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "TaskCloseRequest",
                ReferenceType = ReferenceType.Task,
                ReferenceId = ev.TaskId,
                UserIds = ev.Recipients
            });
        }
    }
}
