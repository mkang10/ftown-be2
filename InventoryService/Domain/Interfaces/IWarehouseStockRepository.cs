using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWareHousesStockRepository
    {
        Task<int> GetStockQuantityAsync(int warehouseId, int variantId);
        Task<int> GetTotalStockByVariantAsync(int variantId);
        //Task<List<WareHousesStock>> GetWareHouseStocksByVariantAsync(int variantId);
        Task<bool> UpdateStockAfterOrderAsync(int warehouseId, List<(int VariantId, int Quantity)> stockUpdates);
    }
}
