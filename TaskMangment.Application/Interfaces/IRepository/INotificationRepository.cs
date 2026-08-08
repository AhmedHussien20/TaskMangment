
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.IRepository
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetUnreadAsync(int userId);
        Task<List<Notification>> GetByTaskAsync(int userId, int taskId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task MarkAsReadByTaskAsync(int userId, int taskId);
    }

}
