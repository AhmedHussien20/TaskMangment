using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Offer;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IOfferService
    {
        Task<ApiResponse<PagedResponse<OfferGetDto>>> GetAllAsync(OfferRequest request);
        Task<ApiResponse<OfferGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<OfferGetDto>> AddAsync(OfferAddEditDto dto);
        Task<ApiResponse<OfferGetDto>> UpdateAsync(int id, OfferAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> AssignOfferToStudentsAsync(int OfferId, OfferAssignStudentsDto dto);

    }
}
