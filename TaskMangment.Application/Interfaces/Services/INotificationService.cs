using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(int userId, string message, bool sendEmail, bool sendWhatsApp, int? taskId);
        Task<List<Notification>> GetUnreadAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }
}
