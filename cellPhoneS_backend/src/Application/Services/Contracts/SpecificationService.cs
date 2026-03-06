using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface SpecificationService
{
    // Admin operations
    Task<ServiceResult<long>> CreateSpecification(CreateSpecificationRequest request, string userId);
    Task<ServiceResult<long>> AddSpecificationDetail(long specificationId, CreateSpecificationDetailRequest request, string userId);
    Task<ServiceResult<string>> DeleteSpecification(long specificationId, string userId);
    
    // Public/User operations
    Task<ServiceResult<List<SpecificationDTO>>> GetAllSpecifications();
    Task<ServiceResult<SpecificationDTO>> GetSpecificationById(long specificationId);
}
