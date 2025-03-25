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
        Task<Import> AddAsync(Import import);
        Task<Import?> GetByIdAsync(int importId);
        Task UpdateAsync(Import import);
        Task<Account?> GetAccountByIdAsync(int accountId);

        Task<PagedResult<Import>> GetImportsAsync(InventoryImportFilterDto filter);


        Task<Import> GetImportByIdAsync(int importId);


    }
}

