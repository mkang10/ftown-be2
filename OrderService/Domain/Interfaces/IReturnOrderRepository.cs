using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IReturnOrderRepository
    {
        Task CreateReturnOrderAsync(ReturnOrder returnOrder);
        //Task AddReturnOrderMediaAsync(List<ReturnOrderMedium> mediaList);
        //Task<ReturnOrder?> GetReturnOrderByIdAsync(int returnOrderId);
        Task<List<ReturnOrder>> GetReturnOrdersByAccountIdAsync(int accountId);
        Task UpdateReturnOrderStatusAsync(int returnOrderId, string status);
        Task AddReturnOrderItemsAsync(List<ReturnOrderItem> returnOrderItems);
    }
}
