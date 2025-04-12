using Application.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class DeleteShippingAddressHandler
    {
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<DeleteShippingAddressHandler> _logger;

        public DeleteShippingAddressHandler(
            IShippingAddressRepository shippingAddressRepository,
            IOrderRepository orderRepository,
            ILogger<DeleteShippingAddressHandler> logger)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<ResponseDTO> Handle(int shippingAddressId)
        {
            var existing = await _shippingAddressRepository.GetByIdAsync(shippingAddressId);
            if (existing == null)
            {
                return new ResponseDTO(true, "Không có địa chỉ đó tồn tại");
            }

            // Tìm các đơn hàng đang dùng địa chỉ
            var relatedOrders = await _orderRepository.GetOrdersByShippingAddressId(shippingAddressId);

            foreach (var order in relatedOrders)
            {
                order.ShippingAddressId = null;
            }

            await _orderRepository.UpdateRangeAsync(relatedOrders);

            // Xóa địa chỉ
            await _shippingAddressRepository.DeleteAsync(existing);

            _logger.LogInformation("Đã xóa địa chỉ ID {ShippingAddressId} và cập nhật {Count} đơn hàng", shippingAddressId, relatedOrders.Count);

            return new ResponseDTO(true, "Xóa địa chỉ thành công và cập nhật các đơn hàng liên quan.");
        }
    }
}
