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
    public class ShippingAddressRepository : IShippingAddressRepository
    {
        private readonly FtownContext _context;
        public ShippingAddressRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<ShippingAddress?> GetByIdAsync(int addressId)
        {
            return await _context.ShippingAddresses
                                 .FirstOrDefaultAsync(a => a.AddressId == addressId);
        }

        public async Task CreateAsync(ShippingAddress address)
        {
            _context.ShippingAddresses.Add(address);
            // Nếu sử dụng UnitOfWork, có thể không gọi SaveChanges ngay ở đây
            await _context.SaveChangesAsync();
        }
    }
}
