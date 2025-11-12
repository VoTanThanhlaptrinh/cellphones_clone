using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/img")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost("upload")]
        public IActionResult UploadImage(IFormFile file)
        {
            // Placeholder for image upload logic
            throw new NotImplementedException();
        }
        [HttpGet("{imageId}")]
        public IActionResult GetImage(long imageId)
        {
            // Placeholder for image retrieval logic
            throw new NotImplementedException();
        }
        [HttpDelete("{imageId}")]
        public IActionResult DeleteImage(long imageId)
        {
            // Placeholder for image deletion logic
            throw new NotImplementedException();
        }
        [HttpPut("{imageId}")]
        public IActionResult UpdateImage(long imageId, IFormFile file)
        {
            // Placeholder for image update logic
            throw new NotImplementedException();
        }
    }
}
