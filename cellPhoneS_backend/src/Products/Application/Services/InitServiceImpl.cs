using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Implement;

public class InitServiceImpl : InitService
{
    // private readonly CategoryService _categoryService;
    private readonly ICategoryRepository _categoryRepository;
    public InitServiceImpl(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<HomeViewModel> InitHomePage()
    {
        return await _categoryRepository.InitHomeByCategories();
    }
}
    