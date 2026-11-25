using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
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

    public async Task<long> CreateSeries(CreateSeriesRequest request, string userId)
    {
        RecordStatuses.EnsureWritable(request.Status);
        var brand = await _brandRepository.GetByIdAsync(request.BrandId);
        if (brand == null || brand.Status == "deleted")
            throw new NotFoundException("Brand not found");

        var existingNames = await _seriesRepository.FindAsync(s => s.Name == request.Name && s.BrandId == request.BrandId && s.Status != "deleted");
        if (existingNames.Any())
            throw new ConflictException("Series with same name already exists in this brand");

        var series = new Series
        {
            BrandId = request.BrandId,
            Name = request.Name,
            SlugName = SlugHelper.GenerateSlug(request.Name),
            Status = request.Status ?? "active",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };

        await _seriesRepository.AddAsync(series);
        await _seriesRepository.SaveChangesAsync();

        return series.Id;
    }

    public async Task UpdateSeries(long seriesId, UpdateSeriesRequest request, string userId)
    {
        RecordStatuses.EnsureWritable(request.Status);
        var series = await _seriesRepository.GetByIdAsync(seriesId);
        if (series == null || series.Status == "deleted")
            throw new NotFoundException("Series not found");

        if (request.BrandId.HasValue && request.BrandId != series.BrandId)
        {
            var brand = await _brandRepository.GetByIdAsync(request.BrandId.Value);
            if (brand == null || brand.Status == "deleted")
                throw new NotFoundException("New brand not found");
            series.BrandId = request.BrandId.Value;
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != series.Name)
        {
            var existingNames = await _seriesRepository.FindAsync(s => s.Name == request.Name && s.BrandId == series.BrandId && s.Id != seriesId && s.Status != "deleted");
            if (existingNames.Any())
                throw new ConflictException("Series with same name already exists in this brand");

            series.Name = request.Name;
            series.SlugName = SlugHelper.GenerateSlug(request.Name);
        }

        if (request.Status != null)
        {
            series.Status = request.Status;
        }

        series.UpdateDate = DateTime.UtcNow;
        series.UpdateBy = userId;

        await _seriesRepository.UpdateAsync(series);
        await _seriesRepository.SaveChangesAsync();
    }

    public async Task DeleteSeries(long seriesId, string userId)
    {
        var series = await _seriesRepository.GetByIdAsync(seriesId);
        if (series == null || series.Status == "deleted")
            throw new NotFoundException("Series not found");

        series.Status = "deleted";
        series.UpdateDate = DateTime.UtcNow;
        series.UpdateBy = userId;

        await _seriesRepository.UpdateAsync(series);
        await _seriesRepository.SaveChangesAsync();
    }

    public async Task RestoreSeries(long seriesId, string userId)
    {
        var series = await _seriesRepository.GetByIdAsync(seriesId);
        if (series == null)
            throw new NotFoundException("Series not found");
        if (series.Status != RecordStatuses.Deleted)
            throw new ConflictException("Dòng sản phẩm không nằm trong thùng rác.");

        var brand = await _brandRepository.GetByIdAsync(series.BrandId);
        if (brand == null || brand.Status == RecordStatuses.Deleted)
            throw new ConflictException("Thương hiệu của dòng sản phẩm đã bị xoá, hãy khôi phục thương hiệu trước.");
        var sameName = await _seriesRepository.FindAsync(s => s.Name == series.Name && s.BrandId == series.BrandId && s.Id != seriesId && s.Status != RecordStatuses.Deleted);
        if (sameName.Any())
            throw new ConflictException("Đã có dòng sản phẩm khác trùng tên trong thương hiệu, không thể khôi phục.");

        series.Status = RecordStatuses.Active;
        series.UpdateDate = DateTime.UtcNow;
        series.UpdateBy = userId;

        await _seriesRepository.UpdateAsync(series);
        await _seriesRepository.SaveChangesAsync();
    }

    public async Task<PagedResult<SeriesResponse>> GetAllSeries(int pageIndex, int pageSize, string? status = "active")
    {
        var query = await _seriesRepository.FindAsync(s => status == null || s.Status == status);
        
        var seriesList = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = query.Count();

        var resultItems = new List<SeriesResponse>();
        foreach (var s in seriesList)
        {
            var b = await _brandRepository.GetByIdAsync(s.BrandId);
            resultItems.Add(new SeriesResponse(s.Id, s.Name, s.BrandId, b?.Name, s.Status));
        }

        var pagedResult = new PagedResult<SeriesResponse>(resultItems, totalCount, pageIndex, pageSize);
        return pagedResult;
    }

    public async Task<List<SeriesResponse>> GetSeriesByBrand(long brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.Status == "deleted")
            throw new NotFoundException("Brand not found");

        var seriesList = await _seriesRepository.FindAsync(s => s.BrandId == brandId && s.Status != "deleted");

        var result = seriesList.Select(s => new SeriesResponse(s.Id, s.Name, s.BrandId, brand.Name, s.Status)).ToList();
        return result;
    }

    public async Task<SeriesResponse> GetSeriesById(long seriesId)
    {
        var series = await _seriesRepository.GetByIdAsync(seriesId);
        if (series == null || series.Status == "deleted")
            throw new NotFoundException("Series not found");

        var brand = await _brandRepository.GetByIdAsync(series.BrandId);

        var result = new SeriesResponse(series.Id, series.Name, series.BrandId, brand?.Name, series.Status);
        return result;
    }
}
