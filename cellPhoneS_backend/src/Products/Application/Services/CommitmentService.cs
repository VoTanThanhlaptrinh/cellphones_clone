using cellPhoneS_backend.DTOs;

namespace cellphones_backend.Services;

public record CommitmentItemDto(long Id, long ProductId, string Context, string Status);

public interface CommitmentService
{
    // Admin operations
    Task<long> CreateCommitment(CreateCommitmentRequest request, string userId);
    Task<List<long>> CreateMultipleCommitments(CreateMultipleCommitmentsRequest request, string userId);
    Task UpdateCommitment(long commitmentId, string newContext, string userId);
    Task DeleteCommitment(long commitmentId, string userId);
    Task<List<CommitmentItemDto>> GetProductCommitmentDetails(long productId);
    
    // Public/User operations
    Task<List<string>> GetProductCommitments(long productId);
}
