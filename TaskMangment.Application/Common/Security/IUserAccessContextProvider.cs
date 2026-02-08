using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Common.Security
{
    public interface IUserAccessContextProvider
    {
        Task<UserAccessContext> GetAsync(int employeeId);
    }

}
