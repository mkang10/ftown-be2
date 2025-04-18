﻿using Application.Interfaces;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Mvc;
using System;
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
    }

}

