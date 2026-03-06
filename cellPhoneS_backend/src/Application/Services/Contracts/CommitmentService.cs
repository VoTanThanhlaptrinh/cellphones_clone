using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface CommitmentService
{
    // Admin operations
    Task<ServiceResult<long>> CreateCommitment(CreateCommitmentRequest request, string userId);
    Task<ServiceResult<List<long>>> CreateMultipleCommitments(CreateMultipleCommitmentsRequest request, string userId);
    Task<ServiceResult<string>> UpdateCommitment(long commitmentId, string newContext, string userId);
    Task<ServiceResult<string>> DeleteCommitment(long commitmentId, string userId);
    
    // Public/User operations
    Task<ServiceResult<List<string>>> GetProductCommitments(long productId);
}
