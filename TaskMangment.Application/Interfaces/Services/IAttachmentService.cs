using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Attachment;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IAttachmentService
    {
        Task<ApiResponse<PagedResponse<AttachmentGetDto>>> GetAllAsync(AttachmentRequest request);
        Task<ApiResponse<AttachmentGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<AttachmentGetDto>> AddAsync(AttachmentAddDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
