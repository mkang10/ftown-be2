using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Response.OrderDTO;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<PaginatedResponseDTO<OrderWithBuyerDTO>> GetAllOrdersWithAssignmentsAsync(
            int page,
            int pageSize,
            string? orderStatus = null,
            DateTime? orderStartDate = null,
            DateTime? orderEndDate = null,
            int? shopManagerId = null,
            int? staffId = null,
            DateTime? assignmentStartDate = null,
            DateTime? assignmentEndDate = null);

        Task<OrderAssignment?> GetByOrderIdAsync(int orderId);
        Task SaveChangesAsync();

        Task<Order?> GetByIdAsync(int orderId);
      
    }
}
