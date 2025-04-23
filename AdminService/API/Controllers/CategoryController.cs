using Application.UseCases;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Domain.DTO.Response;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Category = Domain.Entities.Category;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly GetAllCategoryHandler _handler;

        public CategoriesController(GetAllCategoryHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _handler.GetAllAsync();
                var response = new ResponseDTO<IEnumerable<Category>>(result, true, "Lấy danh sách thành công");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseDTO<string>(null, false, $"Lỗi: {ex.Message}");
                return StatusCode(500, response);
            }
        }
    }

}
