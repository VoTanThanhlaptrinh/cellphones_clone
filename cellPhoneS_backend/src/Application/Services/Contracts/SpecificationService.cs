using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface SpecificationService
{
    // Admin operations
    Task<long> CreateSpecification(CreateSpecificationRequest request, string userId);
    Task<long> AddSpecificationDetail(long specificationId, CreateSpecificationDetailRequest request, string userId);
    Task DeleteSpecification(long specificationId, string userId);
    
    // Public/User operations
    Task<List<SpecificationDTO>> GetAllSpecifications();
    Task<SpecificationDTO> GetSpecificationById(long specificationId);
}
