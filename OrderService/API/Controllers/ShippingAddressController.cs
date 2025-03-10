﻿using Application.DTO.Request;
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

        public ShippingAddressController(IShippingAddressRepository shippingAddressRepository,
                                         ILogger<ShippingAddressController> logger)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShippingAddress([FromBody] CreateShippingAddressRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                Email =request.Email,
                IsDefault = request.IsDefault ?? false
            };

            await _shippingAddressRepository.CreateAsync(newAddress);
            return CreatedAtAction(nameof(GetShippingAddressById), new { id = newAddress.AddressId }, newAddress);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShippingAddressById(int id)
        {
            var address = await _shippingAddressRepository.GetByIdAsync(id);
            if (address == null)
                return NotFound();
            return Ok(address);
        }
    }

}
