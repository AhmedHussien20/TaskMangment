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
    public class TaskExtendApproveEmailHandler : IEventHandler<TaskExtendApproveEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public TaskExtendApproveEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(TaskExtendApproveEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "TaskExtensionApproved",
                ReferenceType = ReferenceType.TaskExtensionRequestApproved,
                RecipientType = RecipientType.Employee,
                ReferenceId = ev.RequestId,
                UserIds = ev.AssignedEmployeeIds
            });
        }
    }
}
