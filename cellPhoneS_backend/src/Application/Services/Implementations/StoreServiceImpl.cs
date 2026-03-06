using System;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class StoreServiceImpl : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IStoreHouseRepository _storeHouseRepository;
    public StoreServiceImpl(IStoreRepository storeRepository, IStoreHouseRepository storeHouseRepository)
    {
        _storeRepository = storeRepository;
        _storeHouseRepository = storeHouseRepository;
    }
    public Task<List<Store>> AllocateAllStockAsync(List<CartDetail> cartItems, string customerProvince)
    {
        return _storeRepository.AllocateAllStockAsync(cartItems, customerProvince);
    }

    public Task<List<Store>> GetInventoryForStoreAsync(List<CartDetail> cartItems, long storeHouseId)
    {
        return _storeRepository.GetInventoryForStoreAsync(cartItems, storeHouseId);
    }

    public async Task<ServiceResult<List<StoreView>>> GetStoreViewsAsync()
    {
        return ServiceResult<List<StoreView>>.Success(await _storeHouseRepository.GetStoreViewsAsync(), "Store views retrieved successfully");
    }
}
