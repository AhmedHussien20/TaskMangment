using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IWhatsAppService
    {
        Task SendTaskAssignedNotification(string phone, string userName, string message);
    }
}
