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
    public class OfferSentEmailHandler : IEventHandler<OfferSentEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public OfferSentEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }

        public async Task Handle(OfferSentEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "OfferSent",
                ReferenceType = ReferenceType.CourseOffer,
                ReferenceId = ev.OfferId,
                UserIds = ev.AssignedStudentsIds
            });
        }
    }
}
