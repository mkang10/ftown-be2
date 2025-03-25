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
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly FtownContext _dbContext;

        public OrderDetailRepository(FtownContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OrderDetail> GetOrderDetailById(int orderDetailId)
        {
            return await _dbContext.OrderDetails
                .Include(od => od.Order) // Include Order để có thể kiểm tra trạng thái
                .FirstOrDefaultAsync(od => od.OrderDetailId == orderDetailId);
        }
    }

}
