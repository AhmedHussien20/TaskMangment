 
using TaskMangment.Application.Common.ApiRequests.Notification;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(NotificationCommand command);
    }

}
