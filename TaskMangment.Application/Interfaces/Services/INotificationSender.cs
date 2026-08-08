using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface INotificationSender
    {
        Task SendWebAsync(int userId, string message, int? taskId, int notificationId);
       
    }

}
