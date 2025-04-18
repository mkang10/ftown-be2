using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWarehouseStockRepos
    {
        Task<WareHousesStock?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<WareHousesStock>> GetByWarehouseIdAsync(int warehouseId);
        Task<bool> HasStockAsync(int ProductId, int sizeId, int ColorId);

    }
}
