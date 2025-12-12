using System;
using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Repositories;
using cellphones_backend.Services;
using cellPhoneS_backend.DTOs;
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

    public async Task<ServiceResult<HomeViewModel>> InitHomePage()
    {
        return ServiceResult<HomeViewModel>.Success(await _categoryRepository.InitHomeByCategories(), "success");
    }
}
    