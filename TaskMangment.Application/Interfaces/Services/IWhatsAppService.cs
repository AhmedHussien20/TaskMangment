using System.Collections.Generic;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Notification;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IWhatsAppService
    {
        Task SendNotificationAsync(
            string phone,
            string userName,
            string message,
            IReadOnlyList<WhatsAppAttachment>? attachments = null);
    }
}
