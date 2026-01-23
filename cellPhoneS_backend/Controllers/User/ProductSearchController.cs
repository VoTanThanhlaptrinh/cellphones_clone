using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Controllers.User
{
    [Route("api/")]
    [ApiController]
    public class ProductSearchController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public ProductSearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<ProductIndexModel>>>> SearchProducts([FromQuery] string keyword)
        {
            var cleanKeyword = keyword.Trim().ToLower();

            var results = await _context.ProductSearchResults
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM ""mv_ProductSearch""
                    WHERE 
                        -- 1. Bắt trường hợp gõ dở (Autocomplete): Tìm chuỗi chứa keyword
                        ""SearchVector"" ILIKE ('%' || unaccent({cleanKeyword}) || '%') 
                        OR 
                        -- 2. Bắt trường hợp sai chính tả (Fuzzy): Tìm chuỗi gần giống
                        ""SearchVector"" % unaccent({cleanKeyword})
                    ORDER BY 
                        similarity(""SearchVector"", unaccent({cleanKeyword})) DESC
                    LIMIT 5")
                .AsNoTracking()
                .ToListAsync();
            if (results.Count == 0)
            {
                return HandleResult(ServiceResult<List<ProductIndexModel>>.Fail("No products found matching the keyword.", ServiceErrorType.NotFound));
            }
            var productModels = results.Select(p => new ProductIndexModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice
            }).ToList();
            return HandleResult(ServiceResult<List<ProductIndexModel>>.Success(productModels, "Products retrieved successfully."));
        }
    }
}