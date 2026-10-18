using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellphones_backend.Services;

public interface DemandService
{
    // Admin operations
    Task<long> CreateDemand(CreateDemandRequest request, string userId);
    Task UpdateDemand(long demandId, UpdateDemandRequest request, string userId);
    Task DeleteDemand(long demandId, string userId);
    Task RestoreDemand(long demandId, string userId);
    
    // Public/User operations
    Task<PagedResult<DemandResponse>> GetAllDemands(int pageIndex, int pageSize, string? status);
    Task<List<DemandResponse>> GetDemandsByCategory(long categoryId);
    Task<DemandResponse> GetDemandById(long demandId);
}
