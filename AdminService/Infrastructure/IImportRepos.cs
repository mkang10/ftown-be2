using Domain.DTO.Enum;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class InventoryImportRepository : IImportRepos
    {
        private readonly FtownContext _context;

        public InventoryImportRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Import> AddAsync(Import import)
        {
            await _context.Imports.AddAsync(import);
            await _context.SaveChangesAsync();
            return import;
        }

        public async Task<Import?> GetByIdAsync(int importId)
        {
            return await _context.Imports
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }

        public async Task UpdateAsync(Import import)
        {
            _context.Imports.Update(import);
            await _context.SaveChangesAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.FindAsync(accountId);
        }


        public async Task<PagedResult<Import>> GetImportsAsync(InventoryImportFilterDto filter)
        {
            // Bắt đầu từ IQueryable để xây dựng query
            var query = _context.Imports
                .Include(ii => ii.CreatedByNavigation)
                .Include(ii => ii.ImportDetails)
                    .ThenInclude(detail => detail.ImportStoreDetails)
                .AsQueryable();

            // Áp dụng các điều kiện filter (nếu có)
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(ii => ii.Status.ToLower().Contains(filter.Status.ToLower()));

            if (filter.CreatedBy.HasValue)
                query = query.Where(ii => ii.CreatedBy == filter.CreatedBy.Value);

            if (filter.CreatedDateFrom.HasValue)
                query = query.Where(ii => ii.CreatedDate >= filter.CreatedDateFrom.Value);

            if (filter.CreatedDateTo.HasValue)
                query = query.Where(ii => ii.CreatedDate <= filter.CreatedDateTo.Value);

            if (!string.IsNullOrEmpty(filter.ReferenceNumber))
                query = query.Where(ii => ii.ReferenceNumber.Contains(filter.ReferenceNumber));

            if (filter.TotalCostMin.HasValue)
                query = query.Where(ii => ii.TotalCost >= filter.TotalCostMin.Value);

            if (filter.TotalCostMax.HasValue)
                query = query.Where(ii => ii.TotalCost <= filter.TotalCostMax.Value);

            if (filter.ApprovedDateFrom.HasValue)
                query = query.Where(ii => ii.ApprovedDate >= filter.ApprovedDateFrom.Value);

            if (filter.ApprovedDateTo.HasValue)
                query = query.Where(ii => ii.ApprovedDate <= filter.ApprovedDateTo.Value);

            if (filter.CompletedDateFrom.HasValue)
                query = query.Where(ii => ii.CompletedDate >= filter.CompletedDateFrom.Value);

            if (filter.CompletedDateTo.HasValue)
                query = query.Where(ii => ii.CompletedDate <= filter.CompletedDateTo.Value);

            // Lấy tổng số bản ghi
            var totalCount = await query.CountAsync();

            // Sắp xếp dựa trên enum và IsDescending
            switch (filter.SortField)
            {
                case InventoryImportSortField.ImportId:
                    query = filter.IsDescending
                        ? query.OrderByDescending(ii => ii.ImportId)
                        : query.OrderBy(ii => ii.ImportId);
                    break;

                case InventoryImportSortField.CreatedDate:
                    query = filter.IsDescending
                        ? query.OrderByDescending(ii => ii.CreatedDate)
                        : query.OrderBy(ii => ii.CreatedDate);
                    break;

                case InventoryImportSortField.TotalCost:
                    query = filter.IsDescending
                        ? query.OrderByDescending(ii => ii.TotalCost)
                        : query.OrderBy(ii => ii.TotalCost);
                    break;

                case InventoryImportSortField.Status:
                    query = filter.IsDescending
                        ? query.OrderByDescending(ii => ii.Status)
                        : query.OrderBy(ii => ii.Status);
                    break;

                case InventoryImportSortField.ReferenceNumber:
                    query = filter.IsDescending
                        ? query.OrderByDescending(ii => ii.ReferenceNumber)
                        : query.OrderBy(ii => ii.ReferenceNumber);
                    break;

                default:
                    // Nếu không xác định, mặc định sắp xếp theo ImportId
                    query = query.OrderBy(ii => ii.ImportId);
                    break;
            }

            // Áp dụng phân trang
            var data = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Import>
            {
                Data = data,
                TotalCount = totalCount
            };
        }


        public async Task<Import> GetImportByIdAsync(int importId)
        {
            return await _context.Imports
                .Include(i => i.CreatedByNavigation)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(pv => pv.Product) // Include để lấy ProductName
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                        .ThenInclude(s => s.StaffDetail)
                            .ThenInclude(sd => sd.Account)  // Include bảng Account
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                        .ThenInclude(s => s.WareHouse)
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }

        public async Task<Import?> GetByIdAssignAsync(int importId)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(detail => detail.ImportStoreDetails)
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }

     

      
    }
}
