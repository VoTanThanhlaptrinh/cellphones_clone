using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface SeriesService
{
    // Admin operations
    Task<ServiceResult<long>> CreateSeries(CreateSeriesRequest request, string userId);
    Task<ServiceResult<string>> UpdateSeries(long seriesId, UpdateSeriesRequest request, string userId);
    Task<ServiceResult<string>> DeleteSeries(long seriesId, string userId);
    
    // Public/User operations
    Task<ServiceResult<PagedResult<SeriesResponse>>> GetAllSeries(int pageIndex, int pageSize, string? status = "active");
    Task<ServiceResult<List<SeriesResponse>>> GetSeriesByBrand(long brandId);
    Task<ServiceResult<SeriesResponse>> GetSeriesById(long seriesId);
}
