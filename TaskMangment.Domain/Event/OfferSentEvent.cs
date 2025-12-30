using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Domain.Event
{
    public class OfferSentEvent
    {
        public int OfferId { get; }
        public string OfferTitle { get; }
        public List<int> AssignedStudentsIds { get; }

        public OfferSentEvent(int offerId, string offerTitle, List<int> assignedStudentsIds)
        {
            OfferId = offerId;
            OfferTitle = offerTitle;
            AssignedStudentsIds = assignedStudentsIds;
        }
    }
}
