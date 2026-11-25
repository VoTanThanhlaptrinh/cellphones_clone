using cellphones_backend.Models;

namespace cellphones_backend.Repositories;

public interface IImageRepository : IRepository<Image>
{
    /// <summary>True nếu ảnh còn được sản phẩm hoặc nhu cầu (demand) tham chiếu.</summary>
    Task<bool> IsReferencedAsync(long imageId);
}
