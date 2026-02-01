using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IOfferSendService
    {
        Task<byte[]> GenerateOfferPdfBytesAsync(int offerId);

    }
}
