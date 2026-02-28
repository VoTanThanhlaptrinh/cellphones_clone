using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement
{
    /// <summary>
    /// Service implementation for managing store-related operations
    /// </summary>
    public class StoreServiceImpl : IStoreService
    {
        private readonly IStoreHouseRepository _storeHouseRepository;

        public StoreServiceImpl(IStoreHouseRepository storeHouseRepository)
        {
            _storeHouseRepository = storeHouseRepository;
        }

        /// <summary>
        /// Retrieves all store locations grouped by city and district
        /// </summary>
        /// <returns>A service result containing a list of store views</returns>
        public async Task<ServiceResult<List<StoreView>>> GetStoreViewsAsync()
        {
            var storeViews = await _storeHouseRepository.GetStoreViews();
            
            if (storeViews == null || storeViews.Count == 0)
            {
                return ServiceResult<List<StoreView>>.Fail("No stores found", ServiceErrorType.NotFound);
            }

            return ServiceResult<List<StoreView>>.Success(storeViews, "Store views retrieved successfully");
        }
    }
}
