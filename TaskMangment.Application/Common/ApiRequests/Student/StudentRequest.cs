using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Student
{
    public class StudentRequest : BaseApiRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
