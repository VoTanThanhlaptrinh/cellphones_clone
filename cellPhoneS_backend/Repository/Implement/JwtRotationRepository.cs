using System;
using cellphones_backend.Data;
using cellphones_backend.Repositories;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Repository.Interfaces;

namespace cellPhoneS_backend.Repository.Implement;

public class JwtRotationRepository : BaseRepository<JwtRotation>, IJwtRotationRepository
{
    public JwtRotationRepository(ApplicationDbContext context) : base(context)
    {
    }
}
