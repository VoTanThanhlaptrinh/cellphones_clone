using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.User
{
    [Route("api/images")]
    [ApiController]
    public class ImageController : BaseController
    {
        private readonly ImageService _imageService;

        public ImageController(ImageService imageService)
        {
            _imageService = imageService;
        }

        // GET: api/images/{imageId}
        // Retrieves image details and blob URL for public viewing
        [HttpGet("{imageId}")]
        public async Task<ActionResult<ApiResponse<ImageDTO>>> GetImage(long imageId)
        {
            var result = await _imageService.GetImage(imageId);
            return HandleResult(result);
        }
    }
}
