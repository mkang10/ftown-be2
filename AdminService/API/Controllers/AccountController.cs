using Application.DTO.Request;
using Application.DTO.Response;
using Application.Enum;
using Application.Interfaces;
using Application.UseCases;
using Domain.Commons;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Application.DTO.Response.MessageRespondDTO<T>;

internal class T
{
}

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserManagementService _service;
        private readonly AuthAdminHandler _authservice;

        private readonly CreateAccountShopManagerDetail _createStaffOrShopmanager;

        public AccountController(IUserManagementService service, AuthAdminHandler authservice, CreateAccountShopManagerDetail createStaffOrShopmanager)
        {
            _service = service;
            _authservice = authservice;
            _createStaffOrShopmanager = createStaffOrShopmanager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var result = await _service.getAccountInfoById(id);
                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<object>(null, false, "User not found.");
                    return NotFound(notFoundResponse);
                }
                var successResponse = new MessageRespondDTO<object>(result, true, "User retrieved successfully.");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUserAccount([FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _service.GetAllUserAscyn(paginationParameter);

                if (result == null)
                {
                    var notFoundResponse = new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString());
                    return NotFound(notFoundResponse);
                }
                else
                {
                    var metadata = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    };

                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                }
                var successResponse = new MessageRespondDTO<object>(result, true, StatusSuccess.Success.ToString());
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateUserRequestWithPasswordDTO user)
        {
            try
            {
                var data = await _createStaffOrShopmanager.createUserStaffOrShopManager(user);
                if (data == null)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }
                return Ok(new MessageRespondDTO<CreateUserFullResponseDTO>(data, true, StatusSuccess.Success.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageRespondDTO<object>(null, false, ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            var response = new MessageResponseButNoData();

            try
            {
                var result = await _service.deleteUser(id);
                if (result)
                {
                    return Ok(new MessageRespondDTO<object>(null, true, StatusSuccess.Success.ToString()));

                }
                return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));

            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, CreateUserRequestWithPasswordDTO user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }

                bool isUpdated = await _service.updateUser(id, user);

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
                var errorResponse = new MessageRespondDTO<object>(null, false,  ex.Message);
                return BadRequest(errorResponse);
            }
        }

        [HttpPut("banned/{id}")]
        public async Task<IActionResult> BanOrActiveUser(int id, BanUserRequestDTO user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new MessageRespondDTO<object>(null, false, StatusSuccess.Wrong.ToString()));
                }

                bool isUpdated = await _service.banUser(id, user);

                if (isUpdated)
                {
                    return Ok(new MessageRespondDTO<object>(null, false, StatusSuccess.Success.ToString()));
                }
                else
                {
                    return NotFound(new MessageRespondDTO<object>(null, false, "Wrong Id to update!"));
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
                return BadRequest(errorResponse);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReq loginDTO)
        {
            try
            {
                var response = await _authservice.AuthenticateAsync(loginDTO.email, loginDTO.Password);

                if (response == null)
                {
                    // Không tìm thấy user hoặc thông tin đăng nhập không chính xác
                    return NotFound(new ResponseDTO<object>(null, false, "Tài khoản hoặc mật khẩu không chính xác!"));
                }

                // Giả sử response.Account có thuộc tính IsActive để xác định trạng thái tài khoản
                if (response.Account == null || response.Account.IsActive != true)
                {
                    return StatusCode(403, new ResponseDTO<object>(null, false, "Tài khoản đang bị vô hiệu hóa!"));
                }


                // Đăng nhập thành công
                return Ok(new ResponseDTO<object>(response, true, "Đăng nhập thành công!"));
            }
            catch (Exception ex)
            {
                // Ở đây bạn có thể log exception (ex) vào hệ thống log của doanh nghiệp
                return StatusCode(500, new ResponseDTO<object>(null, false, "Đã có lỗi xảy ra từ phía server. Vui lòng thử lại sau!"));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestCapcha ps)
        {
            var success = await _authservice.ForgotPasswordAsync(ps);
            if (!success)
                return NotFound("Email không tồn tại trong hệ thống.");

            return Ok("Mật khẩu mới đã được gửi về email của bạn.");
        }


    }
}
