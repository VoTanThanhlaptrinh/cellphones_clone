using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class DemandServiceImpl : DemandService
{
    private readonly IDemandRepository _demandRepository;
    private readonly ICategoryRepository _categoryRepository;

    public DemandServiceImpl(
        IDemandRepository demandRepository, 
        ICategoryRepository categoryRepository)
    {
        _demandRepository = demandRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<long> CreateDemand(CreateDemandRequest request, string userId)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            throw new NotFoundException("Category not found");

        var existingNames = await _demandRepository.FindAsync(d => d.Name == request.Name && d.CategoryId == request.CategoryId && d.Status != "deleted");
        if (existingNames.Any())
            throw new ConflictException("Demand with same name already exists in this category");

        var demand = new Demand
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            SlugName = SlugHelper.GenerateSlug(request.Name),
            Status = request.Status ?? "active",
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreateBy = userId,
            UpdateBy = userId
        };

        await _demandRepository.AddAsync(demand);
        await _demandRepository.SaveChangesAsync();

        return demand.Id;
    }

    public async Task UpdateDemand(long demandId, UpdateDemandRequest request, string userId)
    {
        var demand = await _demandRepository.GetByIdAsync(demandId);
        if (demand == null || demand.Status == "deleted")
            throw new NotFoundException("Demand not found");

        if (request.CategoryId.HasValue && request.CategoryId != demand.CategoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
            if (category == null)
                throw new NotFoundException("New category not found");
            demand.CategoryId = request.CategoryId.Value;
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != demand.Name)
        {
            var existingNames = await _demandRepository.FindAsync(d => d.Name == request.Name && d.CategoryId == demand.CategoryId && d.Id != demandId && d.Status != "deleted");
            if (existingNames.Any())
                throw new ConflictException("Demand with same name already exists in this category");

            demand.Name = request.Name;
            demand.SlugName = SlugHelper.GenerateSlug(request.Name);
        }

        if (request.Status != null)
        {
            demand.Status = request.Status;
        }

        demand.UpdateDate = DateTime.Now;
        demand.UpdateBy = userId;

        await _demandRepository.UpdateAsync(demand);
        await _demandRepository.SaveChangesAsync();
    }

    public async Task DeleteDemand(long demandId, string userId)
    {
        var demand = await _demandRepository.GetByIdAsync(demandId);
        if (demand == null || demand.Status == "deleted")
            throw new NotFoundException("Demand not found");

        demand.Status = "deleted";
        demand.UpdateDate = DateTime.Now;
        demand.UpdateBy = userId;

        await _demandRepository.UpdateAsync(demand);
        await _demandRepository.SaveChangesAsync();
    }

    public async Task<PagedResult<DemandResponse>> GetAllDemands(int pageIndex, int pageSize, string? status = "active")
    {
        var query = await _demandRepository.FindAsync(d => status == null || d.Status == status);
        
        var demands = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = query.Count();

        var resultItems = demands.Select(d => new DemandResponse(d.Id, d.Name!, d.SlugName)).ToList();
        var pagedResult = new PagedResult<DemandResponse>(resultItems, totalCount, pageIndex, pageSize);

        return pagedResult;
    }

    public async Task<List<DemandResponse>> GetDemandsByCategory(long categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.Status == "deleted")
            throw new NotFoundException("Category not found");

        var demands = await _demandRepository.FindAsync(d => d.CategoryId == categoryId && d.Status != "deleted");

        var result = demands.Select(d => new DemandResponse(d.Id, d.Name!, d.SlugName)).ToList();
        return result;
    }

    public async Task<DemandResponse> GetDemandById(long demandId)
    {
        var demand = await _demandRepository.GetByIdAsync(demandId);
        if (demand == null || demand.Status == "deleted")
            throw new NotFoundException("Demand not found");

        var result = new DemandResponse(demand.Id, demand.Name!, demand.SlugName);
        return result;
    }
}
