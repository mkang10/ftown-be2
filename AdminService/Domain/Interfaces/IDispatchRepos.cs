using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDispatchRepos { 
        void Add(Dispatch dispatch);
        Task SaveChangesAsync();
        Task<Dispatch?> GetDispatchByTransferIdAsync(int transferId);
        Task<int> GetApprovedOutboundQuantityAsync(int warehouseId, int variantId);
        // duc anh
        public Task<Dispatch> GetJSONDispatchById(int id);

        Task<StoreExportStoreDetail> GetStoreExportStoreDetailById(int importId);
        Task ReloadAsync(Dispatch dispatch);

        Task AddAsync(Dispatch dispatch);

    }
}
