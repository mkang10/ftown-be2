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
        Task UpdateOrderAsync(Order order);  
        Task<Order?> GetOrderWithDetailsAsync(int orderId); 
        Task SaveOrderDetailsAsync(List<OrderDetail> orderDetails);
        Task<List<Order>> GetOrdersByStatusAsync(string? status, int? accountId);
        Task<Order?> GetOrderByIdAsync(long orderId);
        Task<Order> GetOrderItemsWithOrderIdAsync(int orderId);
        Task<List<Order>> GetReturnableOrdersAsync(int accountId);
        Task UpdateOrderStatusAsync(int orderId, string newStatus);
        Task CreateAssignmentAsync(OrderAssignment assignment);
    }
}
