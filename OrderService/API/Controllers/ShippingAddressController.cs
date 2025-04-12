using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/shippingaddresses")]
    [ApiController]
    public class ShippingAddressController : ControllerBase
    {
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly ILogger<ShippingAddressController> _logger;
        private readonly ShippingCostHandler _shippingCostHandler;
        private readonly GetShippingAddressHandler _shippingAddressHandler;
        
        public ShippingAddressController(IShippingAddressRepository shippingAddressRepository,
                                         ILogger<ShippingAddressController> logger,
                                         ShippingCostHandler shippingCostHandler,
                                         GetShippingAddressHandler shippingAddressHandler)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _logger = logger;
            _shippingCostHandler = shippingCostHandler;
            _shippingAddressHandler = shippingAddressHandler;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<ShippingAddressResponse>>> CreateShippingAddress([FromBody] CreateShippingAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(false, "Invalid request"));
            }

            var response = await _shippingAddressHandler.CreateShippingAddressHandler(request);

            return CreatedAtAction(
                nameof(GetShippingAddressById), // bạn cần định nghĩa thêm phương thức này để dùng với CreatedAtAction
                new { id = response.Data.AddressId },
                response
            );
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<ShippingAddress>>> GetShippingAddressById(int id)
        {
            var address = await _shippingAddressRepository.GetByIdAsync(id);

            if (address == null)
            {
                return Ok(new ResponseDTO<ShippingAddress>(
                    null,
                    true,
                    "Địa chỉ không tồn tại"
                ));
            }

            return Ok(new ResponseDTO<ShippingAddress>(
                address,
                true,
                "Lấy địa chỉ thành công"
            ));
        }



        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<ResponseDTO<List<ShippingAddress>>>> GetShippingAddressesByAccountId(int accountId)
        {
            var addresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(accountId);

            if (addresses == null || !addresses.Any())
            {
                // Trả về danh sách rỗng, status true, message rõ ràng
                return Ok(new ResponseDTO<List<ShippingAddress>>(
                    new List<ShippingAddress>(),
                    true,
                    "Chưa có địa chỉ nào"
                ));
            }

            return Ok(new ResponseDTO<List<ShippingAddress>>(
                addresses,
                true,
                "Lấy danh sách địa chỉ thành công"
            ));
        }



        [HttpGet("cost")]
        public IActionResult GetShippingCost([FromQuery] string city, [FromQuery] string district)
        {
            var shippingCost = _shippingCostHandler.CalculateShippingCost(city, district);
            return Ok(new ResponseDTO<decimal>(shippingCost, true, "Shipping cost calculated successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDTO<ShippingAddressResponse>>> UpdateShippingAddress(int id,
                                                                                    [FromBody] UpdateShippingAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(false, "Invalid request"));
            }

            var response = await _shippingAddressHandler.UpdateShippingAddressHandler(id, request);

            if (response.Data == null)
            {
                return NotFound(response); // Trả về 404 nếu không tìm thấy địa chỉ
            }

            return Ok(response); // Trả về 200 OK với DTO dạng chuẩn
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseDTO>> DeleteShippingAddress(int id)
        {
            var result = await _shippingAddressHandler.DeleteShippingAddressHandler(id);

            return Ok(new ResponseDTO(true, result.Message));
        }


    }


}
