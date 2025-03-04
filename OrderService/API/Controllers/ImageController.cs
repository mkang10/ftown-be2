using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        public ImageController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Vui lòng chọn một file hợp lệ.");
            }

            using var stream = file.OpenReadStream();
            var imageUrl = await _cloudinaryService.UploadImageAsync(stream, file.FileName);

            return Ok(new { Url = imageUrl });
        }

        [HttpDelete("delete/{publicId}")]
        public async Task<IActionResult> DeleteImage(string publicId)
        {
            var result = await _cloudinaryService.DeleteImageAsync(publicId);
            return result ? Ok("Xóa thành công") : BadRequest("Không thể xóa ảnh");
        }
    }
}
