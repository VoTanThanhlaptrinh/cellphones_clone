using System;
using cellphones_backend.Data;
using cellphones_backend.Repositories;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.src.Infrastructure.Persistence.Repositories.Contracts;

namespace cellPhoneS_backend.src.Infrastructure.Persistence.Repositories.Implementations;

public class FeeRepository : BaseRepository<Fee>, IFeeRepository
{
    public FeeRepository(ApplicationDbContext context) : base(context)
    {
    }
}
