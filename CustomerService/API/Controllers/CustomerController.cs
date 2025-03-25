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
        public async Task<ActionResult<ResponseDTO<CustomerProfileResponse>>> GetCustomerProfile(int accountId)
        {
            var result = await _getCustomerProfileService.GetCustomerProfile(accountId);
            if (result == null)
            {
                return NotFound(new ResponseDTO<CustomerProfileResponse>(null, false, "Customer not found"));
            }
            return Ok(new ResponseDTO<CustomerProfileResponse>(result, true, "Customer profile retrieved successfully."));
        }

        [HttpPut("edit-profile/{accountId}")]
        public async Task<ActionResult<ResponseDTO<EditProfileResponse>>> EditProfile(int accountId, [FromBody] EditProfileRequest request)
        {
            var result = await _editProfileHandler.EditProfile(accountId, request);
            if (!result.Success)
            {
                return NotFound(new ResponseDTO<EditProfileResponse>(result, false, "Edit profile failed"));
            }
            return Ok(new ResponseDTO<EditProfileResponse>(result, true, "Edit profile successful"));
        }
    }
}
