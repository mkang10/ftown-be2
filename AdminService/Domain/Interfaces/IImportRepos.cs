using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IImportRepos
    {
        Task<decimal?> GetLatestCostPriceAsync(int productId, int sizeId, int colorId);

        Task<Import> AddAsync(Import import);
        Task<Import?> GetByIdAsync(int importId);
        Task UpdateAsync(Import import);
        Task<Account?> GetAccountByIdAsync(int accountId);

        Task<PagedResult<Import>> GetImportsAsync(InventoryImportFilterDto filter);

        Task SaveChangesAsync();
        Task<Import> GetImportByIdAsync(int importId);

        void Add(Import import);

        Task<Import> GetByIdAsyncWithDetails(int id);

        Task<PaginatedResponseDTO<Warehouse>> GetAllWarehousesAsync(int page, int pageSize);

        Task<Warehouse> GetWareHouseByIdAsync(int id);

        Task<Import?> GetImportByTransferIdAsync(int transferId);

    }
}

