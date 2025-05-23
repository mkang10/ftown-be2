﻿using Domain.Entities;
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
    public class ReturnOrderRepository : IReturnOrderRepository
    {
        private readonly FtownContext _context;

        public ReturnOrderRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task CreateReturnOrderAsync(ReturnOrder returnOrder)
        {
            _context.ReturnOrders.Add(returnOrder);
            await _context.SaveChangesAsync();
        }


        public async Task<List<ReturnOrder>> GetReturnOrdersByAccountIdAsync(int accountId)
        {
            return await _context.ReturnOrders.Where(r => r.AccountId == accountId).ToListAsync();
        }
        public async Task AddReturnOrderItemsAsync(List<ReturnOrderItem> returnOrderItems)
        {
            _context.ReturnOrderItems.AddRange(returnOrderItems);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateReturnOrderStatusAsync(int returnOrderId, string status)
        {
            var returnOrder = await _context.ReturnOrders.FirstOrDefaultAsync(r => r.ReturnOrderId == returnOrderId);
            if (returnOrder != null)
            {
                returnOrder.Status = status;
                returnOrder.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
