using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>Kiểm duyệt đánh giá sản phẩm: ẩn/hiện, xoá. Mọi action cần <c>review.moderate</c>.</summary>
    [Route("api/admin/reviews")]
    [ApiController]
    [RequirePermission(Permissions.ReviewModerate)]
    public class AdminReviewController : BaseController
    {
        private readonly IAdminReviewService _adminReviewService;

        public AdminReviewController(IAdminReviewService adminReviewService)
        {
            _adminReviewService = adminReviewService;
        }

        // GET: api/admin/reviews?productId=&rating=&status=&search=&sortBy=&sortDir=&from=&to=&pageIndex=&pageSize=
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminReviewView>>>> GetReviews([FromQuery] AdminReviewFilter filter)
        {
            var result = await _adminReviewService.GetReviews(filter);
            return Ok(new ApiResponse<PagedResult<AdminReviewView>>("Success", result));
        }

        // PATCH: api/admin/reviews/{id}/status  { "status": "visible" | "hidden" }
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(int id, [FromBody] UpdateReviewStatusRequest request)
        {
            await _adminReviewService.UpdateStatus(id, request);
            return Ok(new ApiResponse<string>("Cập nhật trạng thái đánh giá thành công", ""));
        }

        // DELETE: api/admin/reviews/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            await _adminReviewService.Delete(id);
            return Ok(new ApiResponse<string>("Xoá đánh giá thành công", ""));
        }
    }
}
