using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.DTOs;

namespace TaskMangment.Application.Common.ApiRequests.Discount
{
    public class CompanyDiscountsRequest: BaseApiRequest
    {
        public PeriodDto Period { get; set; } = new();

    }
}
