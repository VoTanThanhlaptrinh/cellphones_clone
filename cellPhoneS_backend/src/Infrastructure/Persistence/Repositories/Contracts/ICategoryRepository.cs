using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    public Task<HomeViewModel> InitHomeByCategories();

    public Task<List<ProductView>> GetProductOfCategory(long categoryId, int size, int page);

    public Task<CategoryDetailView> GetCategoryDetail(long categoryId, int size);
    public Task<CategoryDetailView> GetCategoryDetailBySlug(string slugName, int size);
}
