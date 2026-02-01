using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Implement;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers.User
{
    [Route("api/")]
    [ApiController]
    public class ProductSearchController : BaseController
    {
        private readonly ProductSearchService _service;
        public ProductSearchController(ProductSearchService service)
        {
            this._service = service;
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> SearchProducts([FromQuery] string keyword)
        {
            // string cleanKeyword = StringHelper.RemoveSign(keyword).ToLower().Trim();
            var results = await this._service.SearchAsync(keyword);
            // var results = await _context.ProductSearchResults
            //     .FromSqlInterpolated($@"
            //         SELECT * FROM (
            //             -- BƯỚC 1: Ưu tiên tìm chính xác (Match) - Cực nhanh
            //             SELECT *, 
            //                 1.0 as RankScore -- Điểm tuyệt đối
            //             FROM ""mv_ProductSearch""
            //             WHERE ""SearchVector"" LIKE ('%' || {cleanKeyword} || '%')
                        
            //             UNION ALL
                        
            //             -- BƯỚC 2: Tìm gần đúng (Fuzzy) - Chỉ chạy khi cần thiết
            //             SELECT *, 
            //                 similarity(""SearchVector"", {cleanKeyword}) as RankScore
            //             FROM ""mv_ProductSearch""
            //             WHERE ""SearchVector"" % {cleanKeyword} 
            //             -- Loại bỏ những cái đã tìm thấy ở Bước 1 để tránh trùng
            //             AND NOT (""SearchVector"" LIKE ('%' || {cleanKeyword} || '%'))
            //         ) AS UnifiedResult
            //         ORDER BY RankScore DESC
            //         LIMIT 5")
            //     .AsNoTracking()
            //     .ToListAsync();
            if (results.Count == 0)
            {
                return HandleResult(ServiceResult<List<ProductView>>.Fail("No products found matching the keyword.", ServiceErrorType.NotFound));
            }
            var productModels = results.Select(p => new ProductView(
                p.Id,
                p.ImageUrl,
                p.Name,
                p.BasePrice,
                p.SalePrice)  
            ).ToList();
            return HandleResult(ServiceResult<List<ProductView>>.Success(productModels, "Products retrieved successfully."));
        }
    }
}