using cellphones_backend.Models;
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

    public async Task<long> CreateCommitment(CreateCommitmentRequest request, string userId)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            throw new NotFoundException($"Không tìm thấy sản phẩm có mã {request.ProductId}.");

        var commitment = new Commitment
        {
            ProductCommitmentId = request.ProductId,
            Context = request.Context?.Trim(),
            Status = "active",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };
        await _commitmentRepository.AddAsync(commitment);
        await _commitmentRepository.SaveChangesAsync();
        return commitment.Id;
    }

    public async Task<List<long>> CreateMultipleCommitments(CreateMultipleCommitmentsRequest request, string userId)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            throw new NotFoundException($"Không tìm thấy sản phẩm có mã {request.ProductId}.");

        if (request.Commitments == null || request.Commitments.Count == 0)
            throw new BadRequestException("Danh sách cam kết không được để trống.");

        var ids = new List<long>();
        foreach (var text in request.Commitments.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            var commitment = new Commitment
            {
                ProductCommitmentId = request.ProductId,
                Context = text.Trim(),
                Status = "active",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                CreateBy = userId,
                UpdateBy = userId
            };
            await _commitmentRepository.AddAsync(commitment);
            await _commitmentRepository.SaveChangesAsync();
            ids.Add(commitment.Id);
        }
        return ids;
    }

    public async Task UpdateCommitment(long commitmentId, string newContext, string userId)
    {
        if (string.IsNullOrWhiteSpace(newContext))
            throw new BadRequestException("Nội dung cam kết không được để trống.");

        var commitment = await _commitmentRepository.GetByIdAsync(commitmentId);
        if (commitment == null || commitment.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy cam kết #{commitmentId}.");

        commitment.Context = newContext.Trim();
        commitment.UpdateDate = DateTime.UtcNow;
        commitment.UpdateBy = userId;
        await _commitmentRepository.UpdateAsync(commitment);
        await _commitmentRepository.SaveChangesAsync();
    }

    public async Task DeleteCommitment(long commitmentId, string userId)
    {
        var commitment = await _commitmentRepository.GetByIdAsync(commitmentId);
        if (commitment == null || commitment.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy cam kết #{commitmentId}.");

        commitment.Status = "deleted";
        commitment.UpdateDate = DateTime.UtcNow;
        commitment.UpdateBy = userId;
        await _commitmentRepository.UpdateAsync(commitment);
        await _commitmentRepository.SaveChangesAsync();
    }

    public async Task<List<string>> GetProductCommitments(long productId)
    {
        var commitments = await _commitmentRepository.FindAsync(c => c.ProductCommitmentId == productId && c.Status != "deleted");
        return commitments.Select(c => c.Context!).Where(c => c != null).ToList();
    }

    public async Task<List<CommitmentItemDto>> GetProductCommitmentDetails(long productId)
    {
        var commitments = await _commitmentRepository.FindAsync(c => c.ProductCommitmentId == productId && c.Status != "deleted");
        return commitments.Select(c => new CommitmentItemDto(c.Id, c.ProductCommitmentId, c.Context ?? "", c.Status)).ToList();
    }
}
