using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IEmailReplyTokenService
    {
        Task<string> CreateTaskReplyTokenAsync(int taskId, int commentId);
    }

}
