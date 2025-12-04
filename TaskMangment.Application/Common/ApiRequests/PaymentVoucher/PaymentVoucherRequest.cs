using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.PaymentVoucher
{
    public class PaymentVoucherRequest : BaseApiRequest
    {
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
    }
}
