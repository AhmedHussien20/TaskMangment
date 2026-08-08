using Microsoft.EntityFrameworkCore; 
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IRepository<Notification> _notificationRepo;

        public NotificationRepository(IRepository<Notification> notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task AddAsync(Notification notification)
        {
            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnreadAsync(int userId)
        {
            return await _notificationRepo
                .Query()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .ThenByDescending(n => n.Id)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetByTaskAsync(int userId, int taskId)
        {
            return await _notificationRepo
                .GetAll(n => n.UserId == userId && n.TaskId == taskId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(50)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepo.GetByIDAsync(notificationId);
            if (notification == null) return;

            notification.IsRead = true;
            await _notificationRepo.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _notificationRepo
                .Query()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _notificationRepo.SaveChangesAsync();
        }

        public async Task MarkAsReadByTaskAsync(int userId, int taskId)
        {
            var notifications = await _notificationRepo
                .GetAll(n => n.UserId == userId && n.TaskId == taskId && !n.IsRead)
                .ToListAsync();

            if (notifications.Count == 0) return;

            foreach (var n in notifications)
                n.IsRead = true;

            await _notificationRepo.SaveChangesAsync();
        }
    }


}
