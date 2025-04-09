using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWareHouseStockRepos
    {
        
        Task UpdateWarehouseStockAsync(Import import, int staffId);
        Task UpdateDispatchWarehouseStockAsync(Dispatch dispatch, int staffId);


        Task SaveChangesAsync();

        Task UpdateWarehouseStockForSingleDispatchDetailAsync(StoreExportStoreDetail storeDetail, int productVariantId, int staffId);


        Task UpdateWarehouseStockForSingleDetailAsync(ImportStoreDetail storeDetail, int productVariantId, int staffId);



        Task<PaginatedResponseDTO<WarehouseStockDto>> GetAllWareHouse(int page, int pageSize, CancellationToken cancellationToken = default);

    }
}
