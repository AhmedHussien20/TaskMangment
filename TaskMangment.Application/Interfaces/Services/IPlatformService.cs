using TaskMangment.Application.ApiRequests.Platform;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IPlatformService
    {
        Task<ApiResponse<PlatformLoginResponse>> LoginAsync(PlatformLoginRequest request);
        Task<ApiResponse<PagedResponse<OrganizationListItemDto>>> GetOrganizationsAsync(int platformUserId, OrganizationListRequest request);
        Task<ApiResponse<OrganizationListItemDto>> CreateOrganizationAsync(int platformUserId, CreateOrganizationRequest request);
        Task<ApiResponse<OrganizationListItemDto>> UpdateOrganizationAsync(int platformUserId, int organizationId, UpdateOrganizationRequest request);
        Task<ApiResponse<bool>> DeleteOrganizationAsync(int platformUserId, int organizationId);
        Task<ApiResponse<PagedResponse<TraineeCopyDto>>> GetTraineesAsync(int platformUserId, TraineeCopyListRequest request);
        Task<ApiResponse<TraineeCopyDto>> CreateTraineeAsync(int platformUserId, CreateTraineeRequest request);
        Task<ApiResponse<TraineeCopyDto>> UpdateTraineeAsync(int platformUserId, int companyId, UpdateTraineeRequest request);
        Task<ApiResponse<bool>> DeleteTraineeAsync(int platformUserId, int companyId);
        Task<ApiResponse<TraineeCopyDto>> SetTraineeActiveAsync(int platformUserId, int companyId, bool isActive);
        Task<ApiResponse<PagedResponse<PlatformAdminListItemDto>>> GetPlatformAdminsAsync(int platformUserId, PlatformAdminListRequest request);
        Task<ApiResponse<PlatformAdminListItemDto>> CreatePlatformAdminAsync(int platformUserId, CreatePlatformAdminRequest request);
        Task<ApiResponse<PlatformAdminListItemDto>> UpdatePlatformAdminAsync(int platformUserId, int adminId, UpdatePlatformAdminRequest request);
        Task<ApiResponse<bool>> DeletePlatformAdminAsync(int platformUserId, int adminId);
        Task<ApiResponse<PlatformAdminListItemDto>> SetPlatformAdminActiveAsync(int platformUserId, int adminId, bool isActive);
    }
}
