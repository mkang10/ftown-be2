using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Application.Imports.Dto;
using Domain.DTO.Response.Domain.DTO.Response;
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
        Task<StaffDetail?> GetStaffDetailByIdAsync(int staffDetailId);

        Task<PaginatedResponseDTO<ImportStoreDetailDto>> GetImportStoreDetailByStaffDetailAsync(ImportStoreDetailFilterDtO filter);
        void Add(Import import);

        Task<Import> AddAsync(Import inventoryImport);
        Task SaveChangesAsync();
        Task<(IEnumerable<Import>, int)> GetAllImportsAsync(ImportFilterDto filter, CancellationToken cancellationToken);
        Task<Import> GetByIdAsyncWithDetails(int id);
        Task<Import?> GetByIdAssignAsync(int importId);

        Task<Import?> GetByIdAsync(int importId);
        Task<List<Import>> GetAllByOriginalImportIdAsync(int originalImportId);
        Task UpdateAsync(Import import);

        Task<PaginatedResponseDTO<ProductVariant>> GetAllAsync(int page, int pageSize, string search = null);
        Task<PaginatedResponseDTO<ImportStoreDetailDto>> GetStoreDetailsByStaffDetailAsync(ImportStoreDetailFilterDto filter);
        Task ReloadAsync(Import import);

        Task<Transfer> GetTransferByImportIdAsync(int importId);

        IQueryable<ImportDetail> QueryImportDetails();

        /// <summary>
        /// Kiểm tra xem Import có liên quan đến bất kỳ Transfer nào.
        /// </summary>
        Task<bool> HasTransferForImportAsync(int importId);

        /// <summary>
        /// Lấy đối tượng ProductVariant theo ID.
        /// </summary>
        Task<ProductVariant> GetProductVariantByIdAsync(int variantId);
    }

}

