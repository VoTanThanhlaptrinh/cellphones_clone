using System.Security.Claims;
using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/images")]
    [ApiController]
    [RequirePermission(Permissions.MediaWrite)]
    public class AdminImageController : BaseController
    {
        private readonly ImageService _imageService;

        public AdminImageController(ImageService imageService)
        {
            _imageService = imageService;
        }

        // GET: api/admin/images
        // Retrieves all images for admin library
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ImageDTO>>>> GetAllImages()
        {
            var result = await _imageService.GetAllImages();
            return Ok(new ApiResponse<List<ImageDTO>>("Images retrieved successfully", result));
        }

        // POST: api/admin/images
        // Creates an image from a URL and stores a copy on R2 (file-service)
        [HttpPost]
        public async Task<ActionResult<ApiResponse<long>>> CreateImage([FromBody] CreateImageRequest createImageRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _imageService.CreateImage(createImageRequest, userId);
            return Ok(new ApiResponse<long>("Image created successfully", result));
        }

        // POST: api/admin/images/upload/presign
        // Step 1: get a link so the browser can PUT the file straight to R2 (via file-service)
        [HttpPost("upload/presign")]
        public async Task<ActionResult<ApiResponse<PresignImageUploadResponse>>> PresignUpload([FromBody] PresignImageUploadRequest request)
        {
            var result = await _imageService.PresignUpload(request);
            return Ok(new ApiResponse<PresignImageUploadResponse>("Presign successfully", result));
        }

        // POST: api/admin/images/upload/confirm
        // Step 3: after the PUT, confirm the file and save the image record
        [HttpPost("upload/confirm")]
        public async Task<ActionResult<ApiResponse<long>>> ConfirmUpload([FromBody] ConfirmImageUploadRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }

            var result = await _imageService.ConfirmUpload(request, userId);
            return Ok(new ApiResponse<long>("Image uploaded successfully", result));
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
            
            await _imageService.DeleteImage(imageId, userId);
            return Ok(new ApiResponse<string>("Image deleted successfully", ""));
        }
    }
}