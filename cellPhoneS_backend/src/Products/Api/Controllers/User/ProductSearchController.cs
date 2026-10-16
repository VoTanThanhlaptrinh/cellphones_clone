using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
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
            var results = await this._service.SearchAsync(keyword);
            var productModels = results.Select(p => new ProductView(
                p.Id,
                p.ImageUrl,
                p.Name,
                p.BasePrice,
                p.SalePrice)  
            ).ToList();
            return Ok(new ApiResponse<List<ProductView>>("Products retrieved successfully.", productModels));
        }
    }
}