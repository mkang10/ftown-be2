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
    public class PromotionRepository : IPromotionRepository
    {
        private readonly FtownContext _context;
        private readonly IRedisCacheService _cacheService;

        public PromotionRepository(FtownContext context, IRedisCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }
        public async Task<List<Promotion>> GetActiveProductPromotionsAsync()
        {
            return await _context.Promotions
                .Where(p => p.ApplyTo.Trim() == "PRODUCT" // Xóa khoảng trắng
                            && p.Status.Trim() == "ACTIVE" // Xóa khoảng trắng
                            //&& p.StartDate.Date <= DateTime.Now.Date // So sánh ngày
                            //&& p.EndDate.Date >= DateTime.Now.Date // So sánh ngày
                            && p.ApplyValue != null) // Đảm bảo không bị NULL
                .ToListAsync();
        }

    }
}
