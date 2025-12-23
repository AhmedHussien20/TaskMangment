using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Event;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IEmailQueueService
    {
        Task QueueAsync(EmailQueueRequest request);
    }
}
