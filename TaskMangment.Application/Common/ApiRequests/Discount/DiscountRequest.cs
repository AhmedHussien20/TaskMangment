using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Discount
{
    public class DiscountRequest : BaseApiRequest
    {
        public int? EmployeeId { get; set; }
        public int? TaskId { get; set; }
    }
}
