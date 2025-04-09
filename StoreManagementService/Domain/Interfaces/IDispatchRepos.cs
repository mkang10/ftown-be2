using Domain.DTO.Response.Domain.DTO.Response;
using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Request;
using Microsoft.EntityFrameworkCore;
using static Domain.DTO.Request.StoreExportStoreDetailReq;

namespace Domain.Interfaces
{
    public interface IDispatchRepos
    {
        Task<Dispatch?> GetByIdAssignAsync(int dispatchId);
        Task<Dispatch?> GetByIdAsync(int dispatchId);
        Task<List<Dispatch>> GetAllByOriginalDispatchIdAsync(int originalDispatchId);
        Task ReloadAsync(Dispatch dispatch);
        Task SaveChangesAsync();
        Task<Dispatch?> GetByIdDispatchAssignAsync(int dispatchId);

        Task<PaginatedResponseDTO<DispatchResponseDto>> GetAllDispatchAsync(int page, int pageSize, DispatchFilterDto filter);

        Task UpdateAsync(Dispatch dispatch);

        Task<PaginatedResponseDTO<ExportDetailDto>> GetAllExportStoreDetailsAsync(
              int page,
              int pageSize,
              StoreExportStoreDetailFilterDto filter);

    }
}
