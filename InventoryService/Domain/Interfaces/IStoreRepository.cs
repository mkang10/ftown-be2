using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IStoreRepository
    {
        Task<List<Store>> GetAllStoresAsync();
        Task<Store?> GetStoreByIdAsync(int storeId);
        Task<Store> CreateStoreAsync(Store store);
        Task UpdateStoreAsync(Store store);
        Task DeleteStoreAsync(int storeId);
    }
}
