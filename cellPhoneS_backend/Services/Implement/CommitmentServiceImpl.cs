using cellphones_backend.DTOs.Responses;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class CommitmentServiceImpl : CommitmentService
{
    private readonly ICommitmentRepository _commitmentRepository;
    private readonly IProductRepository _productRepository;

    public CommitmentServiceImpl(
        ICommitmentRepository commitmentRepository,
        IProductRepository productRepository)
    {
        _commitmentRepository = commitmentRepository;
        _productRepository = productRepository;
    }

    public async Task<ServiceResult<long>> CreateCommitment(CreateCommitmentRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Verify product exists using _productRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<long>.Fail with NotFound error
        // 3. Create Commitment entity
        // 4. Set ProductCommitmentId = ProductId from request
        // 5. Set Context from request
        // 6. Set Status = "active"
        // 7. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 8. Set CreateBy = UpdateBy = userId
        // 8. Save using _commitmentRepository.AddAsync()
        // 9. Return ServiceResult<long>.Success with new Commitment ID
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<List<long>>> CreateMultipleCommitments(CreateMultipleCommitmentsRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Verify product exists using _productRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<List<long>>.Fail with NotFound error
        // 3. Begin transaction
        // 4. Loop through each commitment text in the list
        // 5. For each, create Commitment entity with ProductId and Context
        // 6. Set Status = "active" for each
        // 7. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 8. Set CreateBy = UpdateBy = userId
        // 8. Save using _commitmentRepository.AddAsync() and collect IDs
        // 9. Commit transaction
        // 10. Return ServiceResult<List<long>>.Success with list of created Commitment IDs
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<string>> UpdateCommitment(long commitmentId, string newContext, string userId)
    {
        // TODO: Implement business logic using the commitmentId, newContext, and userId here
        // 1. Find commitment by ID using _commitmentRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. Validate newContext (not empty)
        // 4. Update Context field
        // 5. Update UpdateDate = DateTime.UtcNow
        // 6. Update UpdateBy = userId
        // 6. Save changes using _commitmentRepository.UpdateAsync()
        // 7. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<string>> DeleteCommitment(long commitmentId, string userId)
    {
        // TODO: Implement business logic using the commitmentId and userId here
        // 1. Find commitment by ID using _commitmentRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. Set Status = "deleted"
        // 4. Update UpdateDate = DateTime.UtcNow
        // 5. Update UpdateBy = userId
        // 5. Save changes using _commitmentRepository.UpdateAsync()
        // 6. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<List<string>>> GetProductCommitments(long productId)
    {
        // TODO: Implement business logic here
        // 1. Verify product exists using _productRepository.GetByIdAsync()
        // 2. Query commitments for this product where Status != "deleted"
        // 3. Select only Context strings
        // 4. If no commitments found, return empty list
        // 5. Return ServiceResult<List<string>>.Success with commitment text list
        
        throw new NotImplementedException();
    }
}
