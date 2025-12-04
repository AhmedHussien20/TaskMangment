using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;

namespace TaskMangment.Application.Common.ApiRequests.Offer
{
    public class OfferRequest : BaseApiRequest
    {
        public string? Title { get; set; }
        public int? CourseId { get; set; }
        public int? SubjectId { get; set; }
    }
}
