using Domain.Commons;
using Domain.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ExcelRepository : IExcelRepo
    {
        private readonly FtownContext _context;

        public ExcelRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> CreateProduct(List<Product> user)
        {
            _context.Products.AddRangeAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Pagination<Product>> GetAllProduct(PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Products.CountAsync();
            Console.WriteLine($"Item Count: {itemCount}"); // In ra số lượng sản phẩm

            var items = await _context.Products
                                    .Include(p => p.Category)
                                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();

            Console.WriteLine($"Items Retrieved: {items.Count}"); // In ra số lượng items đã lấy

            var result = new Pagination<Product>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }


    }
}
