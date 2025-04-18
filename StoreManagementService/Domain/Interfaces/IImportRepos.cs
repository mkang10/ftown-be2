﻿using Domain.DTO.Request;
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
    public interface IImportRepos
    {
        void Add(Import import);
        Task SaveChangesAsync();
        Task<(IEnumerable<Import>, int)> GetAllImportsAsync(ImportFilterDto filter, CancellationToken cancellationToken);
        Task<Import> GetByIdAsyncWithDetails(int id);
        Task<Import?> GetByIdAssignAsync(int importId);

        Task<Import?> GetByIdAsync(int importId);
        Task<List<Import>> GetAllByOriginalImportIdAsync(int originalImportId);
        Task UpdateAsync(Import import);

        Task<PaginatedResponseDTO<ProductVariant>> GetAllAsync(int page, int pageSize);
        Task<PaginatedResponseDTO<ImportStoreDetail>> GetStoreDetailsByStaffDetailAsync(ImportStoreDetailFilterDto filter);
        Task ReloadAsync(Import import);

        Task<Transfer> GetTransferByImportIdAsync(int importId);
    }

}

