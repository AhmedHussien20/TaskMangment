using System.Collections.Generic;
using System.Text.Json;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors.EmailHandlers
{
    public class TaskAssignedEmailHandler : IEventHandler<TaskAssignedEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public TaskAssignedEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(TaskAssignedEvent ev)
        {
            var metadata = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["EmployeeName"] = string.IsNullOrWhiteSpace(ev.SubjectEmployeeName)
                    ? "-"
                    : ev.SubjectEmployeeName,
                ["BranchName"] = string.IsNullOrWhiteSpace(ev.BranchName)
                    ? "-"
                    : ev.BranchName
            });

            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "TaskAssignedToExistingTask",
                ReferenceType = ReferenceType.Task,
                RecipientType = RecipientType.Employee,
                ReferenceId = ev.TaskId,
                UserIds = ev.RecipientIds,
                MetadataJson = metadata
            });
        }
    }
}
