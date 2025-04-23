using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<(List<OrderAssignment> Items, int TotalRecords)>
        GetAllWithFilterAsync(
            OrderAssignmentFilterDto filter,
            int page,
            int pageSize);

        Task<OrderAssignment?> GetByOrderIdAsync(int orderId);
        Task SaveChangesAsync();

        Task<Order?> GetByIdAsync(int orderId);
        Task<OrderAssignment?> GetByIdWithDetailsAsync(int assignmentId);

    }
}
