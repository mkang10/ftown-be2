using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        private readonly FtownContext _context;

        public OrderHistoryRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<List<OrderHistory>> GetOrderHistoryByOrderIdAsync(int orderId)
        {
            return await _context.OrderHistories
                .Where(oh => oh.OrderId == orderId)
                .Include(oh => oh.ChangedByNavigation) // Lấy thông tin người thay đổi
                .OrderByDescending(oh => oh.ChangedDate)
                .ToListAsync();
        }

        public async Task AddOrderHistoryAsync(OrderHistory orderHistory)
        {
            await _context.OrderHistories.AddAsync(orderHistory);
            await _context.SaveChangesAsync();
        }
    }

}
