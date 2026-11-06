using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class SpecificationServiceImpl : SpecificationService
{
    private readonly ISpecificationRepository _specificationRepository;
    private readonly ISpecificationDetailRepository _specificationDetailRepository;

    public SpecificationServiceImpl(
        ISpecificationRepository specificationRepository,
        ISpecificationDetailRepository specificationDetailRepository)
    {
        _specificationRepository = specificationRepository;
        _specificationDetailRepository = specificationDetailRepository;
    }

    public async Task<long> CreateSpecification(CreateSpecificationRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BadRequestException("Tên nhóm thông số không được để trống.");

        var spec = new Specification
        {
            Name = request.Name.Trim(),
            Status = "active",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };
        await _specificationRepository.AddAsync(spec);
        await _specificationRepository.SaveChangesAsync();

        if (request.Details != null && request.Details.Count > 0)
        {
            foreach (var d in request.Details.Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Value)))
            {
                var detail = new SpecificationDetail
                {
                    SpecificationId = spec.Id,
                    Name = d.Name!.Trim(),
                    Value = d.Value!.Trim(),
                    Status = "active",
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    CreateBy = userId,
                    UpdateBy = userId
                };
                await _specificationDetailRepository.AddAsync(detail);
            }
            await _specificationDetailRepository.SaveChangesAsync();
        }

        return spec.Id;
    }

    public async Task<long> AddSpecificationDetail(long specificationId, CreateSpecificationDetailRequest request, string userId)
    {
        var spec = await _specificationRepository.GetByIdAsync(specificationId);
        if (spec == null || spec.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy nhóm thông số #{specificationId}.");

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Value))
            throw new BadRequestException("Tên và giá trị chi tiết không được để trống.");

        var detail = new SpecificationDetail
        {
            SpecificationId = specificationId,
            Name = request.Name.Trim(),
            Value = request.Value.Trim(),
            Status = "active",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };
        await _specificationDetailRepository.AddAsync(detail);
        await _specificationDetailRepository.SaveChangesAsync();
        return detail.Id;
    }

    public async Task DeleteSpecification(long specificationId, string userId)
    {
        var spec = await _specificationRepository.GetByIdAsync(specificationId);
        if (spec == null || spec.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy nhóm thông số #{specificationId}.");

        spec.Status = "deleted";
        spec.UpdateDate = DateTime.UtcNow;
        spec.UpdateBy = userId;
        await _specificationRepository.UpdateAsync(spec);
        await _specificationRepository.SaveChangesAsync();
    }

    public async Task<List<SpecificationDTO>> GetAllSpecifications()
    {
        var specs = await _specificationRepository.FindAsync(s => s.Status != "deleted");
        var specList = specs.OrderByDescending(s => s.CreateDate).ToList();
        var specIds = specList.Select(s => s.Id).ToList();
        var allDetails = await _specificationDetailRepository.FindAsync(d => specIds.Contains(d.SpecificationId) && d.Status != "deleted");
        var detailsBySpec = allDetails.ToLookup(d => d.SpecificationId);

        return specList.Select(s => new SpecificationDTO(
            s.Name,
            detailsBySpec[s.Id].Select(d => new SpecificationDetailDTO(d.Name, d.Value, d.Id)).ToList(),
            s.Id
        )).ToList();
    }

    public async Task<SpecificationDTO> GetSpecificationById(long specificationId)
    {
        var spec = await _specificationRepository.GetByIdAsync(specificationId);
        if (spec == null || spec.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy nhóm thông số #{specificationId}.");

        var details = await _specificationDetailRepository.FindAsync(d => d.SpecificationId == specificationId && d.Status != "deleted");
        return new SpecificationDTO(
            spec.Name,
            details.Select(d => new SpecificationDetailDTO(d.Name, d.Value, d.Id)).ToList(),
            spec.Id
        );
    }
}
