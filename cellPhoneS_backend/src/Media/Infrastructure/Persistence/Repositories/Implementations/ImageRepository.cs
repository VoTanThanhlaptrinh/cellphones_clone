using cellphones_backend.Data;
using cellphones_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class ImageRepository : BaseRepository<Image>, IImageRepository
{
    public ImageRepository(ApplicationDbContext context) : base(context) {}

    public async Task<bool> IsReferencedAsync(long imageId)
        => await _context.ProductImages.AnyAsync(pi => pi.ImageId == imageId)
           || await _context.DemandImages.AnyAsync(di => di.ImageId == imageId);
}
