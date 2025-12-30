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
    public class TaskCommentEmailHandler : IEventHandler<TaskCommentAddedEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public TaskCommentEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(TaskCommentAddedEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "TaskCommentAdded",
                ReferenceType = ReferenceType.TaskComment,
                RecipientType = RecipientType.Employee,
                ReferenceId = ev.CommentId,
                UserIds = ev.Recipients
            });
        }
    }
}
