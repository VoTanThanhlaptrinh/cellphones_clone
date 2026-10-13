using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface CommitmentService
{
    // Admin operations
    Task<long> CreateCommitment(CreateCommitmentRequest request, string userId);
    Task<List<long>> CreateMultipleCommitments(CreateMultipleCommitmentsRequest request, string userId);
    Task UpdateCommitment(long commitmentId, string newContext, string userId);
    Task DeleteCommitment(long commitmentId, string userId);
    
    // Public/User operations
    Task<List<string>> GetProductCommitments(long productId);
}
