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
        // Uploads a new image to Azure Blob Storage and creates an Image entity in the database
        [HttpPost]
        public async Task<ActionResult<ApiResponse<long>>> CreateImage([FromBody] CreateImageRequest createImageRequest)
        {
            var result = await _imageService.CreateImage(createImageRequest);
            return HandleResult(result);
        }

        // POST: api/admin/images/upload
        // Uploads an image file directly from form data
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<long>>> UploadImage([FromForm] IFormFile file, [FromForm] string? alt)
        {
            // TODO: Implement the business logic to upload image file here. I will develop this part.
            // Implementation steps:
            // 1. Validate file size and MIME type
            // 2. Generate unique file name
            // 3. Upload file directly to Azure Blob Storage
            // 4. Get the blob URL
            // 5. Create Image entity with BlobUrl, OriginUrl (same as BlobUrl), MimeType, Name, Alt
            // 6. Save to database
            // 7. Return the new Image ID
            
            return Ok(new ApiResponse<long>(
                "Image upload endpoint is ready but not yet implemented",
                0
            var result = await _imageService.UploadImage(file, alt);
            return HandleResult(result/ 4. If not in use, set Status = "deleted"
            // 5. Optionally delete from blob storage
            // 6. Save changes
            
            return Ok(new ApiResponse<string>(
                "Image deletion endpoint is ready but not yet implemented",
                null!
            var result = await _imageService.DeleteImage(imageId);
            return HandleResult(result