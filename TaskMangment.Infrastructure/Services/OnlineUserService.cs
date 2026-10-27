using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class OnlineUserService : IOnlineUserService
    {
        private readonly ICurrentUserService _currentUser;

        public OnlineUserService(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        public bool IsUserOnline(int userId)
        {
            return NotificationHub.IsUserOnline(
                NotificationHub.ConnectionKey(
                    _currentUser.Schema ?? OperationalSchemaAmbient.Current, _currentUser.CompanyId, userId));
        }
    }

}
