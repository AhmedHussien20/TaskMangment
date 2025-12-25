using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class OnlineUserService : IOnlineUserService
    {
        public bool IsUserOnline(int userId)
        {
            return NotificationHub._onlineUsers.ContainsKey(userId);
        }
    }

}
