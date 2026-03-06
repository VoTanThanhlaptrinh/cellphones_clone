using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/images")]
    [ApiController]
    public class AdminImageController : BaseController
    {
        private readonly ImageService _imageService;

        public AdminImageController(ImageService imageService)
        {
            _imageService = imageService;
        }

        // POST: api/admin/images
        // Creates an image from a URL and uploads it to Azure Blob Storage
        [HttpPost]
        public async Task<ActionResult<ApiResponse<long>>> CreateImage([FromBody] CreateImageRequest createImageRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _imageService.CreateImage(createImageRequest, userId);
            return HandleResult(result);
        }

        // POST: api/admin/images/upload
        // Uploads an image file directly from form data
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<long>>> UploadImage([FromForm] IFormFile file, [FromForm] string? alt)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _imageService.UploadImage(file, alt, userId);
            return HandleResult(result);
        }

        // DELETE: api/admin/images/{imageId}
        // Soft deletes an image (sets status to "deleted")
        [HttpDelete("{imageId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteImage(long imageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _imageService.DeleteImage(imageId, userId);
            return HandleResult(result);
        }
    }
}