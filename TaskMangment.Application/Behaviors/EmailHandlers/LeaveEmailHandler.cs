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
    public class LeaveEmailHandler : IEventHandler<LeaveEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public LeaveEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(LeaveEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "LeaveRequestCreated",
                ReferenceType = ReferenceType.Leave,
                RecipientType = RecipientType.Employee,
                ReferenceId = ev.LeaveId,
                UserIds = new List<int> { ev.EmployeeId }
            });
        }
    }

}
