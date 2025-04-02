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

        public ShippingAddressController(IShippingAddressRepository shippingAddressRepository,
                                         ILogger<ShippingAddressController> logger,
                                         ShippingCostHandler shippingCostHandler)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _logger = logger;
            _shippingCostHandler = shippingCostHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShippingAddress([FromBody] CreateShippingAddressRequest request)
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

            return CreatedAtAction(nameof(GetShippingAddressById),
                new { id = newAddress.AddressId },
                new ResponseDTO<ShippingAddress>(newAddress, true, "Created successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShippingAddressById(int id)
        {
            var address = await _shippingAddressRepository.GetByIdAsync(id);
            if (address == null)
            {
                return NotFound(new ResponseDTO(false, "Shipping address not found"));
            }

            return Ok(new ResponseDTO<ShippingAddress>(address, true, "Success"));
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetShippingAddressesByAccountId(int accountId)
        {
            var addresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(accountId);
            if (addresses == null || !addresses.Any())
            {
                return NotFound(new ResponseDTO(false, "No shipping addresses found"));
            }

            return Ok(new ResponseDTO<List<ShippingAddress>>(addresses, true, "Success"));
        }

        [HttpGet("cost")]
        public IActionResult GetShippingCost([FromQuery] string city, [FromQuery] string district)
        {
            var shippingCost = _shippingCostHandler.CalculateShippingCost(city, district);
            return Ok(new ResponseDTO<decimal>(shippingCost, true, "Shipping cost calculated successfully"));
        }
    }


}
