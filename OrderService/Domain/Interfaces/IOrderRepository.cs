using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrderHistoryByAccountIdAsync(int accountId);
        Task UpdateOrderAsync(Order order);  // ✅ Cập nhật trạng thái đơn hàng
        Task<Order?> GetOrderWithDetailsAsync(int orderId); // ✅ Lấy chi tiết đơn hàng kèm Payment
        Task SaveOrderDetailsAsync(List<OrderDetail> orderDetails);
        Task<List<Order>> GetOrdersByStatusAsync(string status);
        Task<Order?> GetOrderByIdAsync(long orderId);
    }
}
