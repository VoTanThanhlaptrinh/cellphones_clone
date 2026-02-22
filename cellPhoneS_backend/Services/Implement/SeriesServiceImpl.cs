using cellphones_backend.DTOs.Responses;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class SeriesServiceImpl : SeriesService
{
    private readonly ISeriesRepository _seriesRepository;
    private readonly IBrandRepository _brandRepository;

    public SeriesServiceImpl(ISeriesRepository seriesRepository, IBrandRepository brandRepository)
    {
        _seriesRepository = seriesRepository;
        _brandRepository = brandRepository;
    }

    public async Task<ServiceResult<long>> CreateSeries(CreateSeriesRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Verify brand exists using _brandRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<long>.Fail with NotFound error
        // 3. Check if series with same name already exists for this brand
        // 4. Create Series entity
        // 5. Set BrandId, Name from request
        // 6. Set Status = request.Status ?? "active"
        // 7. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 8. Set CreateBy = UpdateBy = userId
        // 6. Save to database using _seriesRepository.AddAsync()
        // 7. Return ServiceResult<long>.Success with new Series ID
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<string>> UpdateSeries(long seriesId, CreateSeriesRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Find series by ID using _seriesRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. If BrandId changed, verify new brand exists
        // 4. Update Name, BrandId, Status if provided
        // 5. Update UpdateDate = DateTime.UtcNow
        // 6. Update UpdateBy = userId
        // 5. Save changes using _seriesRepository.UpdateAsync()
        // 6. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<string>> DeleteSeries(long seriesId, string userId)
    {
        // TODO: Implement business logic using the userId here
        // 1. Find series by ID using _seriesRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. Check if series has products (consider navigation properties)
        // 4. If has dependencies, return error or handle cascade soft delete
        // 5. Set Status = "deleted"
        // 6. Update UpdateDate = DateTime.UtcNow
        // 7. Update UpdateBy = userId
        // 7. Save changes using _seriesRepository.UpdateAsync()
        // 8. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<List<SeriesView>>> GetSeriesByBrand(long brandId)
    {
        // TODO: Implement business logic here
        // 1. Verify brand exists using _brandRepository
        // 2. Query series for this brand where Status != "deleted"
        // 3. Project to SeriesView DTOs
        // 4. If no series found, return empty list or NotFound based on business rules
        // 5. Return ServiceResult<List<SeriesView>>.Success with series list
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<SeriesView>> GetSeriesById(long seriesId)
    {
        // TODO: Implement business logic here
        // 1. Find series by ID with related data (Brand) using _seriesRepository
        // 2. If not found or Status = "deleted", return ServiceResult<SeriesView>.Fail with NotFound error
        // 3. Project to SeriesView DTO
        // 4. Return ServiceResult<SeriesView>.Success
        
        throw new NotImplementedException();
    }
}
