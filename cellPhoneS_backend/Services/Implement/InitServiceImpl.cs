using System;
using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Implement;

public class InitServiceImpl : InitService
{
    // private readonly CategoryService _categoryService;
    private readonly ProductService _productService;
    private readonly IConfiguration _configuration;
    private string[]? categories;
    private int pageSize;
    private readonly ApplicationDbContext _dbContext;
    public InitServiceImpl(ProductService productService, IConfiguration configuration, ApplicationDbContext dbContext)
    {
        _productService = productService;
        _configuration = configuration;
        categories = configuration.GetSection("DefaultSetting:CategoryInit").Get<string[]>();
        pageSize = configuration.GetValue<int>("DefaultSetting:PageSize");
        this._dbContext = dbContext;
    }

    public Task<ApiResponse<HomeViewModel>> InitHomePage()
    {
        var categoriesWithNewestProducts = _dbContext.Categories
        .Where(c => categories!.Contains(c.Name!))
        .Select(category => new
        {
            CategoryName = category.Name!,
            NewestProducts = category.CategoryProducts
                .Select(cp => cp.Product)
                .OrderByDescending(p => p!.CreateDate)
                .Select(product => new ProductView
                (
                    product!.Id,
                    product!.ImageUrl,
                    product.Name,
                    product.BasePrice,
                    product.SalePrice
                ))
                .Take(pageSize)
            // (KHÔNG CÓ .ToList() Ở ĐÂY)
        })
        .ToDictionary(result => result.CategoryName, result => result.NewestProducts);
        return Task.FromResult(new ApiResponse<HomeViewModel>("success", new HomeViewModel(categoriesWithNewestProducts), 200));
    }
}
