using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface SeriesService
{
    // Admin operations
    Task<ServiceResult<long>> CreateSeries(CreateSeriesRequest request, string userId);
    Task<ServiceResult<string>> UpdateSeries(long seriesId, CreateSeriesRequest request, string userId);
    Task<ServiceResult<string>> DeleteSeries(long seriesId, string userId);
    
    // Public/User operations
    Task<ServiceResult<List<SeriesView>>> GetSeriesByBrand(long brandId);
    Task<ServiceResult<SeriesView>> GetSeriesById(long seriesId);
}
