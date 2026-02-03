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
    public class PublicHolidayEmailHandler: IEventHandler<PublicHolidayEvent>
    {
        private readonly IEmailQueueService _emailQueue;

        public PublicHolidayEmailHandler(IEmailQueueService emailQueue)
        {
            _emailQueue = emailQueue;
        }


        public async Task Handle(PublicHolidayEvent ev)
        {
            await _emailQueue.QueueAsync(new EmailQueueRequest
            {
                TemplateKey = "OfficialHoliday",
                ReferenceType = ReferenceType.OfficialHoliday,
                ReferenceId = ev.CalenderId,
                RecipientType = RecipientType.Employee,
                ForAll = true    
            });
        }
    }
}
