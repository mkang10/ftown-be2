using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Response.OrderDTO;

namespace Infrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FtownContext _context;
        public OrderRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponseDTO<OrderWithBuyerDTO>>
     GetAllOrdersWithAssignmentsAsync(
         int page,
         int pageSize,
         string? orderStatus = null,
         DateTime? orderStartDate = null,
         DateTime? orderEndDate = null,
         int? shopManagerId = null,
         int? staffId = null,
         DateTime? assignmentStartDate = null,
         DateTime? assignmentEndDate = null)
        {
            // 2.1. Build base query, include cả Account và OrderAssignments
            var query = _context.Orders
                                .Include(o => o.Account)            // để lấy tên người mua
                                .Include(o => o.OrderAssignments)   // để lấy assignments
                                .AsQueryable();

            // 2.2. Filter theo Order
            if (!string.IsNullOrEmpty(orderStatus))
                query = query.Where(o => o.Status == orderStatus);

            if (orderStartDate.HasValue)
                query = query.Where(o => o.CreatedDate >= orderStartDate.Value);

            if (orderEndDate.HasValue)
                query = query.Where(o => o.CreatedDate <= orderEndDate.Value);

            // 2.3. Filter theo Assignment nếu có
            if (shopManagerId.HasValue || staffId.HasValue ||
                assignmentStartDate.HasValue || assignmentEndDate.HasValue)
            {
                query = query.Where(o => o.OrderAssignments.Any(oa =>
                       (!shopManagerId.HasValue || oa.ShopManagerId == shopManagerId.Value)
                    && (!staffId.HasValue || oa.StaffId == staffId.Value)
                    && (!assignmentStartDate.HasValue || oa.AssignmentDate >= assignmentStartDate.Value)
                    && (!assignmentEndDate.HasValue || oa.AssignmentDate <= assignmentEndDate.Value)
                ));
            }

            // 2.4. Tổng số record
            var totalRecords = await query.CountAsync();

            // 2.5. Phân trang và project ra DTO
            var items = await query
                .OrderBy(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderWithBuyerDTO
                {
                    OrderId = o.OrderId,
                    CreatedDate = o.CreatedDate,
                    Status = o.Status,
                    BuyerName = o.Account.FullName,  // hoặc o.FullName nếu bạn muốn dùng cột này
                    Assignments = o.OrderAssignments
                        .Where(oa =>
                            (!shopManagerId.HasValue || oa.ShopManagerId == shopManagerId.Value)
                         && (!staffId.HasValue || oa.StaffId == staffId.Value)
                         && (!assignmentStartDate.HasValue || oa.AssignmentDate >= assignmentStartDate.Value)
                         && (!assignmentEndDate.HasValue || oa.AssignmentDate <= assignmentEndDate.Value)
                        )
                        .Select(oa => new AssignmentDTO
                        {
                            ShopManagerId = oa.ShopManagerId,
                            StaffId = oa.StaffId,
                            AssignmentDate = oa.AssignmentDate,
                            Comments = oa.Comments
                        })
                        .ToList()
                })
                .ToListAsync();

            // 2.6. Trả về PaginatedResponseDTO
            return new PaginatedResponseDTO<OrderWithBuyerDTO>(
                items, totalRecords, page, pageSize
            );
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
    }
}
