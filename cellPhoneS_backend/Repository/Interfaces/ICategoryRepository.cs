using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    public Task<HomeViewModel> InitHomeByCategories();
}
