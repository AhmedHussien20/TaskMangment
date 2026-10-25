using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IPlatformService
    {
        Task<ApiResponse<PlatformLoginResponse>> LoginAsync(PlatformLoginRequest request);
        Task<ApiResponse<List<OrganizationListItemDto>>> GetOrganizationsAsync(int platformUserId);
        Task<ApiResponse<OrganizationListItemDto>> CreateOrganizationAsync(int platformUserId, CreateOrganizationRequest request);
        Task<ApiResponse<List<TraineeCopyDto>>> GetTraineesAsync(int platformUserId);
        Task<ApiResponse<TraineeCopyDto>> CreateTraineeAsync(int platformUserId, CreateTraineeRequest request);
        Task<ApiResponse<TraineeCopyDto>> SetTraineeActiveAsync(int platformUserId, int companyId, bool isActive);
    }
}
