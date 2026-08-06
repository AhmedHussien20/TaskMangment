using System.Collections.Generic;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Notification;
using TaskMangment.Application.DTOs;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(
            int userId,
            string message,
            bool sendEmail,
            bool sendWhatsApp,
            int? taskId,
            NotificationType type,
            int referenceId,
            string? whatsAppMessage = null,
            IReadOnlyList<WhatsAppAttachment>? whatsAppAttachments = null);
        Task<List<Notification>> GetUnreadAsync(int userId);
        Task<List<TaskNotificationDto>> GetByTaskAsync(int userId, int taskId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task MarkAsReadByTaskAsync(int userId, int taskId);
    }
}
