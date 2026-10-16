using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellphones_backend.Services;

public interface SeriesService
{
    // Admin operations
    Task<long> CreateSeries(CreateSeriesRequest request, string userId);
    Task UpdateSeries(long seriesId, UpdateSeriesRequest request, string userId);
    Task DeleteSeries(long seriesId, string userId);
    
    // Public/User operations
    Task<PagedResult<SeriesResponse>> GetAllSeries(int pageIndex, int pageSize, string? status = "active");
    Task<List<SeriesResponse>> GetSeriesByBrand(long brandId);
    Task<SeriesResponse> GetSeriesById(long seriesId);
}
