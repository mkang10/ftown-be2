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
        private readonly UpdateShippingAddressHandler _updateShippingAddressHandler;
        private readonly DeleteShippingAddressHandler _deleteShippingAddressHandler;
        public ShippingAddressController(IShippingAddressRepository shippingAddressRepository,
                                         ILogger<ShippingAddressController> logger,
                                         ShippingCostHandler shippingCostHandler,
                                         UpdateShippingAddressHandler updateShippingAddressHandler,
                                         DeleteShippingAddressHandler deleteShippingAddressHandler)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _logger = logger;
            _shippingCostHandler = shippingCostHandler;
            _updateShippingAddressHandler = updateShippingAddressHandler;
            _deleteShippingAddressHandler = deleteShippingAddressHandler;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<ShippingAddress>>> CreateShippingAddress([FromBody] CreateShippingAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(false, "Invalid request"));
            }

            var newAddress = new ShippingAddress
            {
                AccountId = request.AccountId,
                Address = request.Address,
                City = request.City,
                Province = request.Province,
                District = request.District,
                Country = request.Country,
                PostalCode = request.PostalCode,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                Email = request.Email,
                IsDefault = request.IsDefault ?? false
            };

            await _shippingAddressRepository.CreateAsync(newAddress);

            var response = new ResponseDTO<ShippingAddress>(
                data: newAddress,
                status: true,
                message: "Created successfully"
            );

            return CreatedAtAction(nameof(GetShippingAddressById), new { id = newAddress.AddressId }, response);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<ShippingAddress>>> GetShippingAddressById(int id)
        {
            var address = await _shippingAddressRepository.GetByIdAsync(id);

            if (address == null)
            {
                return Ok(new ResponseDTO<ShippingAddress>(
                    data: null,
                    status: true,
                    message: "Địa chỉ không tồn tại"
                ));
            }

            return Ok(new ResponseDTO<ShippingAddress>(
                data: address,
                status: true,
                message: "Lấy địa chỉ thành công"
            ));
        }


        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<ResponseDTO<List<ShippingAddress>>>> GetShippingAddressesByAccountId(int accountId)
        {
            var addresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(accountId);

            if (addresses == null || !addresses.Any())
            {
                return Ok(new ResponseDTO<List<ShippingAddress>>(
                    data: new List<ShippingAddress>(),
                    status: true,
                    message: "Chưa có địa chỉ nào"
                ));
            }

            return Ok(new ResponseDTO<List<ShippingAddress>>(
                data: addresses,
                status: true,
                message: "Lấy danh sách địa chỉ thành công"
            ));
        }


        [HttpGet("cost")]
        public IActionResult GetShippingCost([FromQuery] string city, [FromQuery] string district)
        {
            var shippingCost = _shippingCostHandler.CalculateShippingCost(city, district);
            return Ok(new ResponseDTO<decimal>(shippingCost, true, "Shipping cost calculated successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDTO<ShippingAddress>>> UpdateShippingAddress(int id,[FromBody] UpdateShippingAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO(false, "Invalid request"));
            }

            var result = await _updateShippingAddressHandler.Handle(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseDTO>> DeleteShippingAddress(int id)
        {
            var result = await _deleteShippingAddressHandler.Handle(id);
            return Ok(result);
        }

    }


}
