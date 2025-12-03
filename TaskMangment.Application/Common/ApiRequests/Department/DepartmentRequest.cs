using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Department
{
    public class DepartmentRequest : BaseApiRequest
    {
        public string? Name { get; set; }      
        public int? BranchId { get; set; }   
        public int? AreaId { get; set; }
    }
}
