 
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Behaviors.EmailHandlers
{
    public class LeaveRejectedEmailHandler : IEventHandler<LeaveRejectedEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public LeaveRejectedEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(LeaveRejectedEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "LeaveRejected",
                ReferenceType = ReferenceType.Leave,
                RecipientType = RecipientType.Employee,
                ReferenceId = ev.LeaveId,
                UserIds = new List<int> { ev.EmployeeId }
            });
        }
    }

}
