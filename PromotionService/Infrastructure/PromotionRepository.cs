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
    public class PromotionRepository : IPromotionRepository
    {
        private readonly FtownContext _context;

        public PromotionRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<int> CreatePromotionAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion.PromotionId;
        }
        public async Task<bool> UpdatePromotionAsync(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeletePromotionAsync(int promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null) return false;

            _context.Promotions.Remove(promotion);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<List<Promotion>> GetAllPromotionsAsync(string? status)
        {
            return await _context.Promotions
                .Where(p => string.IsNullOrEmpty(status) || p.Status == status)
                .ToListAsync();
        }
        public async Task<Promotion?> GetPromotionByIdAsync(int promotionId)
        {
            return await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromotionId == promotionId);
        }
    }
}
