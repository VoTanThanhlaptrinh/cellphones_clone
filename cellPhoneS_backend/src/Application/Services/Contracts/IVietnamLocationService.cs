using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellPhoneS_backend.Services.Interface;

public interface IVietnamLocationService
{
    Task<ServiceResult<List<VietnamProvinceDto>>> GetLocationsAsync(int depth, CancellationToken ct = default);
    Task WarmUpAsync(CancellationToken ct = default);
}
