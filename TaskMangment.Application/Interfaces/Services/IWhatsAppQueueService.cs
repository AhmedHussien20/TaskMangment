using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IWhatsAppQueueService
    {
        Task QueueAsync(int userId, string message);
    }

}
