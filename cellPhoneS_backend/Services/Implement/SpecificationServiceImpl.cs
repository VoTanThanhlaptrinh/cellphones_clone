using cellphones_backend.DTOs.Responses;
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

    public async Task<ServiceResult<long>> CreateSpecification(CreateSpecificationRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Check if specification with same name already exists
        // 2. Create Specification entity
        // 3. Set Name from request
        // 4. Set Status = "active"
        // 5. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 6. Set CreateBy = UpdateBy = userId
        // 5. Save Specification using _specificationRepository.AddAsync()
        // 6. If Details list provided, loop through and create SpecificationDetail entities
        // 7. Link each detail to the created Specification
        // 8. Save all details using _specificationDetailRepository.AddAsync()
        // 9. Commit transaction
        // 10. Return ServiceResult<long>.Success with new Specification ID
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<long>> AddSpecificationDetail(long specificationId, CreateSpecificationDetailRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Verify specification exists using _specificationRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<long>.Fail with NotFound error
        // 3. Create SpecificationDetail entity
        // 4. Set SpecificationId, Name, Value from request
        // 5. Set Status = "active"
        // 6. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 7. Set CreateBy = UpdateBy = userId
        // 7. Save using _specificationDetailRepository.AddAsync()
        // 8. Return ServiceResult<long>.Success with new SpecificationDetail ID
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<string>> DeleteSpecification(long specificationId, string userId)
    {
        // TODO: Implement business logic using the specificationId and userId here
        // 1. Find specification by ID using _specificationRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. Check if specification is used in products
        // 4. If has dependencies, return error or handle cascade soft delete
        // 5. Set Status = "deleted"
        // 6. Update UpdateDate = DateTime.UtcNow
        // 7. Update UpdateBy = userId
        // 8. Save changes using _specificationRepository.UpdateAsync()
        // 9. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<List<SpecificationDTO>>> GetAllSpecifications()
    {
        // TODO: Implement business logic here
        // 1. Query all specifications where Status != "deleted"
        // 2. Include related SpecificationDetails
        // 3. Project to SpecificationDTO with nested SpecificationDetailDTO list
        // 4. If no specifications found, return empty list or NotFound based on business rules
        // 5. Return ServiceResult<List<SpecificationDTO>>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<SpecificationDTO>> GetSpecificationById(long specificationId)
    {
        // TODO: Implement business logic here
        // 1. Find specification by ID with related SpecificationDetails
        // 2. If not found or Status = "deleted", return ServiceResult<SpecificationDTO>.Fail with NotFound error
        // 3. Project to SpecificationDTO with nested SpecificationDetailDTO list
        // 4. Return ServiceResult<SpecificationDTO>.Success
        
        throw new NotImplementedException();
    }
}
