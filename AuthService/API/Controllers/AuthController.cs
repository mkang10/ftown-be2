using Application.Interfaces;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AuthAdminHandler _service;

        public AuthController(IAuthService authService, AuthAdminHandler service)
        {
            _authService = authService;
            _service = service;
        }





        /// <summary>
        /// Đăng nhập và lấy JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReq loginDTO)
        {
            try
            {
                var response = await _authService.AuthenticateAsync(loginDTO.email, loginDTO.Password);

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


      
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var result = await _authService.AuthenticateWithGoogleAsync(request.IdToken);

            if (result == null || result.Token == null)
            {
                return Unauthorized(new { message = "Đăng nhập Google không thành công hoặc tài khoản bị vô hiệu hoá." });
            }

            return Ok(result);
        }
        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterReq registerDTO)
        {
            try
            {
                var response = await _authService.RegisterAsync(registerDTO);

                if (response == null || string.IsNullOrEmpty(response.Token))
                {
                    // Tên đăng nhập đã tồn tại hoặc đăng ký không thành công
                    return Conflict(new ResponseDTO<object>(null, false, "Tên đăng nhập đã tồn tại!"));
                }

                // Đăng ký thành công
                return Ok(new ResponseDTO<object>(response, true, "Đăng ký thành công!"));
            }
            catch (Exception ex)
            {
                // Log exception (ex) nếu cần thiết
                return StatusCode(500, new ResponseDTO<object>(null, false, "Đã có lỗi xảy ra từ phía server. Vui lòng thử lại sau!"));
            }


        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestCapcha ps)
        {
            if(ps == null)
            {
                throw new ArgumentNullException("Không được để null!");    
            }
            var success = await _service.ForgotPasswordAsync(ps);
            if (!success)
                return NotFound("Email không tồn tại trong hệ thống.");

            return Ok("Mật khẩu mới đã được gửi về email của bạn.");
        }
    }

}

