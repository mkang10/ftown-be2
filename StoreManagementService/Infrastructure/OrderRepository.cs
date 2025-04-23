using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
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
    public class OrderRepository : IOrderRepository
    {
        private readonly FtownContext _context;
        public OrderRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<(List<OrderAssignment>, int)> GetAllWithFilterAsync(
    OrderAssignmentFilterDto f,
    int page,
    int pageSize)
        {
            var q = _context.OrderAssignments
                // Include Order → OrderDetails → ProductVariant → Product
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                // Include ProductVariant → Size
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Size)
                // Include ProductVariant → Color
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Color)
                .AsNoTracking()
                .AsQueryable();

            // 1. Filter OrderAssignment
            if (f.AssignmentId.HasValue)
                q = q.Where(oa => oa.AssignmentId == f.AssignmentId);
            if (f.ShopManagerId.HasValue)
                q = q.Where(oa => oa.ShopManagerId == f.ShopManagerId);
            if (f.StaffId.HasValue)
                q = q.Where(oa => oa.StaffId == f.StaffId);
            if (f.AssignmentDateFrom.HasValue)
                q = q.Where(oa => oa.AssignmentDate >= f.AssignmentDateFrom);
            if (f.AssignmentDateTo.HasValue)
                q = q.Where(oa => oa.AssignmentDate <= f.AssignmentDateTo);
            if (!string.IsNullOrWhiteSpace(f.CommentsContains))
                q = q.Where(oa => oa.Comments.Contains(f.CommentsContains));

            // 2. Filter Order
            if (f.OrderCreatedDateFrom.HasValue)
                q = q.Where(oa => oa.Order.CreatedDate >= f.OrderCreatedDateFrom);
            if (f.OrderCreatedDateTo.HasValue)
                q = q.Where(oa => oa.Order.CreatedDate <= f.OrderCreatedDateTo);
            if (!string.IsNullOrWhiteSpace(f.OrderStatus))
                q = q.Where(oa => oa.Order.Status == f.OrderStatus);
            if (f.MinOrderTotal.HasValue)
                q = q.Where(oa => oa.Order.OrderTotal >= f.MinOrderTotal);
            if (f.MaxOrderTotal.HasValue)
                q = q.Where(oa => oa.Order.OrderTotal <= f.MaxOrderTotal);
            if (!string.IsNullOrWhiteSpace(f.FullNameContains))
                q = q.Where(oa => oa.Order.FullName.Contains(f.FullNameContains));

            // 3. Count & Paging
            var total = await q.CountAsync();
            var data = await q
                .OrderBy(oa => oa.AssignmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<OrderAssignment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderAssignments
                .Include(oa => oa.Order)
                .FirstOrDefaultAsync(oa => oa.OrderId == orderId);
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<OrderAssignment?> GetByIdWithDetailsAsync(int assignmentId)
        {
            return await _context.OrderAssignments
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Size)
                .Include(oa => oa.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductVariant)
                            .ThenInclude(pv => pv.Color)
                .AsNoTracking()
                .FirstOrDefaultAsync(oa => oa.AssignmentId == assignmentId);
        }
    }
}
