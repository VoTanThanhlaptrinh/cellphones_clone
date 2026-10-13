using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace cellPhoneS_backend.Services.Implement;

public class CacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IStoreHouseRepository _storeHouseRepository;

    public CacheService(IMemoryCache memoryCache, IStoreHouseRepository storeHouseRepository)
    {
        _memoryCache = memoryCache;
        _storeHouseRepository = storeHouseRepository;
    }

    public async Task<List<StoreView>> GetStoreViews()
    {
        var data = await _memoryCache.GetOrCreateAsync(
            "StoreHouseInit",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _storeHouseRepository.GetStoreViewsAsync();
            });

        if (data == null)
        {
            throw new NotFoundException("Store views not available");
        }

        return data;
    }
}
