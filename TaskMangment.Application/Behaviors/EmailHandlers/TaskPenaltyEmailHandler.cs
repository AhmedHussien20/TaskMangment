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
    public class TaskPenaltyEmailHandler : IEventHandler<TaskPenaltyEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public TaskPenaltyEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(TaskPenaltyEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "EmployeeDeduction",
                ReferenceType = ReferenceType.EmployeeDeduction,
                ReferenceId = ev.DiscountId,
                UserIds = new List<int> { ev.IssuedtoId }
            });
        }
    }
}
