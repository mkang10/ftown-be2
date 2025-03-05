using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IInventoryImportRepository
    {
        Task<InventoryImport> AddAsync(InventoryImport import);
        Task<InventoryImport?> GetByIdAsync(int importId);
        Task UpdateAsync(InventoryImport import);
        Task<Account?> GetAccountByIdAsync(int accountId);

        Task<List<InventoryImport>> GetAllPendingAsync();

    }
}
