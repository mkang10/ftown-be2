﻿using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
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
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
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
        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }
    }
}
