using Application.Interfaces;
using Domain.DTO.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập và lấy JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReq loginDTO)
        {
            var response = await _authService.AuthenticateAsync(loginDTO.Username, loginDTO.Password);

            if (response == null)
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            if (response.Account.IsActive == false)
            {
                return Forbid("Tài khoản đang bị vô hiệu hóa!");
            }

            return Ok(response);
        }

        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterReq registerDTO)
        {
            var response = await _authService.RegisterAsync(registerDTO);

            if (response.Token == null)
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });
            }

            return Ok(response);
        }
    }
}
