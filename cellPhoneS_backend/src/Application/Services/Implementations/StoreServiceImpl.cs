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
    public Task<List<StoreInventoryDTO>> AllocateAllStockAsync(List<CartDetail> cartItems)
    {
        return _storeRepository.AllocateAllStockAsync(cartItems);
    }

    public Task<List<Store>> GetStoresAsync(List<OrderDetail> orderDetails)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<List<StoreView>>> GetStoreViewsAsync()
    {
        return ServiceResult<List<StoreView>>.Success(await _storeHouseRepository.GetStoreViewsAsync(), "Store views retrieved successfully");
    }

    public async Task UpdateStores(List<StoreInventoryDTO> stores)
    {
        await _storeRepository.UpdateRangeAsync(stores);
    }
}
