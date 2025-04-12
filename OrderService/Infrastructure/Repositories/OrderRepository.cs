using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FtownContext _context;

        public OrderRepository(FtownContext context)
        {
            _context = context;
        }

        public Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            // KHÔNG gọi SaveChangesAsync ở đây
            return Task.FromResult(order);
        }


        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments) // Lấy phương thức thanh toán
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderByIdAsync(long orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        public async Task<List<Order>> GetOrderHistoryByAccountIdAsync(int accountId)
        {
            return await _context.Orders
                .Where(o => o.AccountId == accountId)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments) // ✅ Lấy thêm Payment
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task SaveOrderDetailsAsync(List<OrderDetail> orderDetails)
        {
            _context.OrderDetails.AddRange(orderDetails);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Order>> GetOrdersByStatusAsync(string? status, int? accountId)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (accountId.HasValue)
            {
                query = query.Where(o => o.AccountId == accountId);
            }

            return await query.ToListAsync();
        }
        public async Task<List<Order>> GetReturnableOrdersAsync(int accountId)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Where(o => o.AccountId == accountId &&
                            o.Status == "completed" &&
                            _context.AuditLogs.Any(al =>
                                al.TableName == "Orders" &&
                                al.Operation == "completed" &&
                                al.RecordId == o.OrderId.ToString() &&
                                al.ChangeDate >= sevenDaysAgo))
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            return orders;
        }
        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return;

            order.Status = newStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
        public async Task<Order> GetOrderItemsWithOrderIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        public async Task CreateAssignmentAsync(OrderAssignment assignment)
        {
            _context.OrderAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetOrdersByShippingAddressId(int shippingAddressId)
        {
            return await _context.Orders
                .Where(o => o.ShippingAddressId == shippingAddressId)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Order> orders)
        {
            _context.Orders.UpdateRange(orders);
            await _context.SaveChangesAsync();
        }
    }
}
