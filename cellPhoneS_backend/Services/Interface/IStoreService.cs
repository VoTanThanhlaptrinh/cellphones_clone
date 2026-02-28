using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellPhoneS_backend.Services.Interface
{
    /// <summary>
    /// Service interface for managing store-related operations
    /// </summary>
    public interface IStoreService
    {
        /// <summary>
        /// Retrieves all store locations grouped by city and district
        /// </summary>
        /// <returns>A service result containing a list of store views</returns>
        Task<ServiceResult<List<StoreView>>> GetStoreViewsAsync();
    }
}
