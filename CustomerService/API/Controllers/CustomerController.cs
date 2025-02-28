using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly EditProfileHandler _editProfileHandler;
        private readonly GetCustomerProfileHandler _getCustomerProfileService;
        public CustomerController(EditProfileHandler editProfileHandler, GetCustomerProfileHandler getCustomerProfileService)
        {
            _editProfileHandler = editProfileHandler;
            _getCustomerProfileService = getCustomerProfileService;
        }

        [HttpGet("profile/{accountId}")]
        public async Task<ActionResult<CustomerProfileResponse>> GetCustomerProfile(int accountId)
        {
            var result = await _getCustomerProfileService.GetCustomerProfile(accountId);
            if (result == null)
            {
                return NotFound(new { message = "Customer not found" });
            }
            return Ok(result);
        }

        [HttpPut("edit-profile/{accountId}")]
        public async Task<ActionResult<EditProfileResponse>> EditProfile(int accountId, [FromBody] EditProfileRequest request)
        {
            var result = await _editProfileHandler.EditProfile(accountId, request);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}
