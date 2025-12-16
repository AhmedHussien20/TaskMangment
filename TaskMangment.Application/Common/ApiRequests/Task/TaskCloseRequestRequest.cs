using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Common.ApiRequests.Task
{
    public class TaskCloseRequestRequest :BaseApiRequest
    {
        public int TaskId { get; set; }

    }
}
