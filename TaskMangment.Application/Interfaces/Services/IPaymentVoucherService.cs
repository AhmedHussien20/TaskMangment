using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.PaymentVoucher;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IPaymentVoucherService
    {
        Task<ApiResponse<PagedResponse<PaymentVoucherGetDto>>> GetAllAsync(PaymentVoucherRequest request);
        Task<ApiResponse<PaymentVoucherGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaymentVoucherGetDto>> AddAsync(PaymentVoucherAddEditDto dto, int CompanyId, int CreatedBy);
        Task<ApiResponse<PaymentVoucherGetDto>> UpdateAsync(int id, PaymentVoucherAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
