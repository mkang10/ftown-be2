using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITransferRepos
    {
        void Add(Transfer transfer);
        Task SaveChangesAsync();
        // Thêm các phương thức khác nếu cần (ví dụ: GetById, Update, Delete)
        Task<PaginatedResponseDTO<TransferDto>> GetAllWithPagingAsync(
           int page,
           int pageSize,
           string? filter,
           CancellationToken cancellationToken = default
       );

        //Duc Anh
        public Task<Transfer> GetJSONTransferOrderById(int id);
        //public Task<Import> GetJSONTransferOrderWithImportById(int id);
        //public Task<Dispatch> GetJSONTransferOrderWithDispatchById(int id);

    }


}
