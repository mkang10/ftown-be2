using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IInventoryImportRepos
    {
        Task<InventoryImport> AddAsync(InventoryImport inventoryImport);

        Task<List<InventoryImport>> GetAllAsync(int createdby);


    }
}
