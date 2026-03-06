using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellphones_backend.Services.Interface;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class StoreHouseServiceImpl : StoreHouseService
{
    private readonly IStoreHouseRepository _storeHouseRepository;

    public StoreHouseServiceImpl(IStoreHouseRepository storeHouseRepository)
    {
        _storeHouseRepository = storeHouseRepository;
    }

    public async Task<ServiceResult<long>> CreateStoreHouse(CreateStoreHouseRequest request, string userId)
    {
        var existingStore = await _storeHouseRepository.FindAsync(s => 
            s.Street == request.Street && 
            s.District == request.District && 
            s.City == request.City && 
            s.Status != "deleted");

        if (existingStore.Any())
            return ServiceResult<long>.Fail("A storehouse at this exact address already exists");

        var storeHouse = new StoreHouse
        {
            Street = request.Street,
            District = request.District,
            City = request.City,
            Status = request.Status ?? "active",
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreateBy = userId,
            UpdateBy = userId
        };

        await _storeHouseRepository.AddAsync(storeHouse);
        await _storeHouseRepository.SaveChangesAsync();

        return ServiceResult<long>.Success(storeHouse.Id, "Storehouse created successfully");
    }

    public async Task<ServiceResult<string>> UpdateStoreHouse(long storeHouseId, UpdateStoreHouseRequest request, string userId)
    {
        var storeHouse = await _storeHouseRepository.GetByIdAsync(storeHouseId);
        if (storeHouse == null || storeHouse.Status == "deleted")
            return ServiceResult<string>.Fail("Storehouse not found");

        if (!string.IsNullOrEmpty(request.Street)) storeHouse.Street = request.Street;
        if (!string.IsNullOrEmpty(request.District)) storeHouse.District = request.District;
        if (!string.IsNullOrEmpty(request.City)) storeHouse.City = request.City;

        var existingStore = await _storeHouseRepository.FindAsync(s => 
            s.Street == storeHouse.Street && 
            s.District == storeHouse.District && 
            s.City == storeHouse.City && 
            s.Id != storeHouseId && 
            s.Status != "deleted");

        if (existingStore.Any())
            return ServiceResult<string>.Fail("A storehouse at this resulting address already exists");

        if (request.Status != null)
        {
            storeHouse.Status = request.Status;
        }

        storeHouse.UpdateDate = DateTime.Now;
        storeHouse.UpdateBy = userId;

        await _storeHouseRepository.UpdateAsync(storeHouse);
        await _storeHouseRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Storehouse updated successfully");
    }

    public async Task<ServiceResult<string>> DeleteStoreHouse(long storeHouseId, string userId)
    {
        var storeHouse = await _storeHouseRepository.GetByIdAsync(storeHouseId);
        if (storeHouse == null || storeHouse.Status == "deleted")
            return ServiceResult<string>.Fail("Storehouse not found");

        storeHouse.Status = "deleted";
        storeHouse.UpdateDate = DateTime.Now;
        storeHouse.UpdateBy = userId;

        await _storeHouseRepository.UpdateAsync(storeHouse);
        await _storeHouseRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Storehouse deleted successfully");
    }

    public async Task<ServiceResult<PagedResult<StoreHouseResponse>>> GetAllStoreHouses(int pageIndex, int pageSize, string? status = "active")
    {
        var query = await _storeHouseRepository.FindAsync(s => status == null || s.Status == status);
        
        var storeHouses = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = query.Count();

        if (!storeHouses.Any())
            return ServiceResult<PagedResult<StoreHouseResponse>>.Fail("No storehouses found");

        var resultItems = storeHouses.Select(s => new StoreHouseResponse(s.Id, s.Street, s.District, s.City, s.Status)).ToList();
        var pagedResult = new PagedResult<StoreHouseResponse>(resultItems, totalCount, pageIndex, pageSize);

        return ServiceResult<PagedResult<StoreHouseResponse>>.Success(pagedResult, "Success");
    }

    public async Task<ServiceResult<StoreHouseResponse>> GetStoreHouseById(long storeHouseId)
    {
        var storeHouse = await _storeHouseRepository.GetByIdAsync(storeHouseId);
        if (storeHouse == null || storeHouse.Status == "deleted")
            return ServiceResult<StoreHouseResponse>.Fail("Storehouse not found");

        var result = new StoreHouseResponse(storeHouse.Id, storeHouse.Street, storeHouse.District, storeHouse.City, storeHouse.Status);
        return ServiceResult<StoreHouseResponse>.Success(result, "Success");
    }
}
