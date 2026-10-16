using cellphones_backend.Data;
using cellphones_backend.Models;

namespace cellphones_backend.Repositories;

public class TeacherRepository : BaseRepository<Teacher>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context) {}
}
