using Application.DTO.Request;
using Application.DTO.Response;
using Application.Enum;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTO.Response.MessageRespondDTO<T>;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopManagerDetailController : ControllerBase
    {
        private readonly IUserManagementService _service;

        public ShopManagerDetailController(IUserManagementService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateShopmanagerDetailRequest user)
        {
            try
            {
                var data = await _service.CreateShopmanagerDetail(user);
                if (data == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }
                return Ok(new MessageRespondDTO<CreateShopmanagerDetailRequest>(data, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }

        
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, CreateShopmanagerDetailRequest user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }

                bool isUpdated = await _service.UpdateShopmanagerDetail(id, user);

                if (isUpdated)
                {
                    return Ok(new MessageRespondDTO<object>(null, true, StatusSuccess.Success.ToString()));
                }
                else
                {
                    return NotFound(new MessageRespondDTO<object>(null, false, "Wrong Id to update!"));
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, ex.Message);
                return BadRequest(errorResponse);
            }
        }

    }
}
