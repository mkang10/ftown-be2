using Domain.DTO.Request;
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
        void Add(Import import);
        Task SaveChangesAsync();
        Task<(IEnumerable<Import>, int)> GetAllImportsAsync(ImportFilterDto filter, CancellationToken cancellationToken);

        Task<Import?> GetByIdAssignAsync(int importId);

        Task<Import?> GetByIdAsync(int importId);

        Task UpdateAsync(Import import);

        Task<List<ProductVariant>> GetAllAsync();

        Task<PaginatedResponseDTO<ImportStoreDetail>> GetStoreDetailsByStaffDetailAsync(ImportStoreDetailFilterDto filter);

    }
}

