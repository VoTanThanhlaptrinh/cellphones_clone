using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Requests;
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
        var brand = await _brandRepository.GetByIdAsync(request.BrandId);
        if (brand == null || brand.Status == "deleted")
            return ServiceResult<long>.Fail("Brand not found");

        var existingNames = await _seriesRepository.FindAsync(s => s.Name == request.Name && s.BrandId == request.BrandId && s.Status != "deleted");
        if (existingNames.Any())
            return ServiceResult<long>.Fail("Series with same name already exists in this brand");

        var series = new Series
        {
            BrandId = request.BrandId,
            Name = request.Name,
            SlugName = SlugHelper.GenerateSlug(request.Name),
            Status = request.Status ?? "active",
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreateBy = userId,
            UpdateBy = userId
        };

        await _seriesRepository.AddAsync(series);
        await _seriesRepository.SaveChangesAsync();

        return ServiceResult<long>.Success(series.Id, "Series created successfully");
    }

    public async Task<ServiceResult<string>> UpdateSeries(long seriesId, UpdateSeriesRequest request, string userId)
    {
        var series = await _seriesRepository.GetByIdAsync(seriesId);
        if (series == null || series.Status == "deleted")
            return ServiceResult<string>.Fail("Series not found");

        if (request.BrandId.HasValue && request.BrandId != series.BrandId)
        {
            var brand = await _brandRepository.GetByIdAsync(request.BrandId.Value);
            if (brand == null || brand.Status == "deleted")
                return ServiceResult<string>.Fail("New brand not found");
            series.BrandId = request.BrandId.Value;
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != series.Name)
        {
            var existingNames = await _seriesRepository.FindAsync(s => s.Name == request.Name && s.BrandId == series.BrandId && s.Id != seriesId && s.Status != "deleted");
            if (existingNames.Any())
                return ServiceResult<string>.Fail("Series with same name already exists in this brand");

            series.Name = request.Name;
            series.SlugName = SlugHelper.GenerateSlug(request.Name);
        }

        if (request.Status != null)
        {
            series.Status = request.Status;
        }

        series.UpdateDate = DateTime.Now;
        series.UpdateBy = userId;

        await _seriesRepository.UpdateAsync(series);
        await _seriesRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Series updated successfully");
    }

    public async Task<ServiceResult<string>> DeleteSeries(long seriesId, string userId)
    {
        var series = await _seriesRepository.GetByIdAsync(seriesId);
        if (series == null || series.Status == "deleted")
            return ServiceResult<string>.Fail("Series not found");

        series.Status = "deleted";
        series.UpdateDate = DateTime.Now;
        series.UpdateBy = userId;

        await _seriesRepository.UpdateAsync(series);
        await _seriesRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Series deleted successfully");
    }

    public async Task<ServiceResult<PagedResult<SeriesResponse>>> GetAllSeries(int pageIndex, int pageSize, string? status = "active")
    {
        var query = await _seriesRepository.FindAsync(s => status == null || s.Status == status);
        
        var seriesList = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = query.Count();

        if (!seriesList.Any())
            return ServiceResult<PagedResult<SeriesResponse>>.Fail("No series found");

        var resultItems = new List<SeriesResponse>();
        foreach (var s in seriesList)
        {
            var b = await _brandRepository.GetByIdAsync(s.BrandId);
            resultItems.Add(new SeriesResponse(s.Id, s.Name, s.BrandId, b?.Name, s.Status));
        }

        var pagedResult = new PagedResult<SeriesResponse>(resultItems, totalCount, pageIndex, pageSize);
        return ServiceResult<PagedResult<SeriesResponse>>.Success(pagedResult, "Series retrieved successfully");
    }

    public async Task<ServiceResult<List<SeriesResponse>>> GetSeriesByBrand(long brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.Status == "deleted")
            return ServiceResult<List<SeriesResponse>>.Fail("Brand not found");

        var seriesList = await _seriesRepository.FindAsync(s => s.BrandId == brandId && s.Status != "deleted");
        if (!seriesList.Any())
            return ServiceResult<List<SeriesResponse>>.Fail("No series found in this brand");

        var result = seriesList.Select(s => new SeriesResponse(s.Id, s.Name, s.BrandId, brand.Name, s.Status)).ToList();
        return ServiceResult<List<SeriesResponse>>.Success(result, "Series retrieved successfully");
    }

    public async Task<ServiceResult<SeriesResponse>> GetSeriesById(long seriesId)
    {
        var series = await _seriesRepository.GetByIdAsync(seriesId);
        if (series == null || series.Status == "deleted")
            return ServiceResult<SeriesResponse>.Fail("Series not found");

        var brand = await _brandRepository.GetByIdAsync(series.BrandId);

        var result = new SeriesResponse(series.Id, series.Name, series.BrandId, brand?.Name, series.Status);
        return ServiceResult<SeriesResponse>.Success(result, "Series retrieved successfully");
    }
}
