using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWarehouseRepository
    {
        Task<Warehouse> CreateAsync(Warehouse warehouse);

        Task UpdateAsync(Warehouse warehouse);

        Task<Warehouse?> GetByIdAsync(int warehouseId);

        Task<Warehouse> GetOwnerWarehouseAsync();
        Task<IEnumerable<Warehouse>> GetAllAsync();

    }
}
