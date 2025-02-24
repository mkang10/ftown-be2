using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IStoreStockRepository
    {
        Task<int> GetStockQuantityAsync(int storeId, int variantId);
        Task<int> GetTotalStockByVariantAsync(int variantId);
        Task<List<StoreStock>> GetStoreStocksByVariantAsync(int variantId);
    }
}
