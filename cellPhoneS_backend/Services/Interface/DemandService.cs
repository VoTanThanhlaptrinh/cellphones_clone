using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface DemandService
{
    // Admin operations
    Task<ServiceResult<long>> CreateDemand(CreateDemandRequest request, string userId);
    Task<ServiceResult<string>> UpdateDemand(long demandId, UpdateDemandRequest request, string userId);
    Task<ServiceResult<string>> DeleteDemand(long demandId, string userId);
    
    // Public/User operations
    Task<ServiceResult<PagedResult<DemandResponse>>> GetAllDemands(int pageIndex, int pageSize, string? status);
    Task<ServiceResult<List<DemandResponse>>> GetDemandsByCategory(long categoryId);
    Task<ServiceResult<DemandResponse>> GetDemandById(long demandId);
}
