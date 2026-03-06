using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class ImageRepository : BaseRepository<Image>, IImageRepository
{
    public ImageRepository(ApplicationDbContext context) : base(context) {}
}
