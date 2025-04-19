﻿using Domain.DTO.Enum;
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
        private readonly IProductVarRepos _variantRepo;

        public InventoryImportRepository(FtownContext context, IProductVarRepos variantRepo)
        {
            _context = context;
            _variantRepo = variantRepo;
        }

        public async Task<Import> AddAsync(Import import)
        {
            await _context.Imports.AddAsync(import);
            await _context.SaveChangesAsync();
            return import;
        }

        public async Task<Import?> GetImportByTransferIdAsync(int transferId)
        {
            var transfer = await _context.Transfers
              .AsNoTracking()
              .FirstOrDefaultAsync(t => t.TransferOrderId == transferId);
            if (transfer == null || transfer.ImportId == 0)
                return null;

            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ProductVariant)
                        .ThenInclude(v => v.Color)
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ProductVariant)
                        .ThenInclude(v => v.Size)
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ImportStoreDetails)
                .FirstOrDefaultAsync(i => i.ImportId == transfer.ImportId);
        }
        public async Task<Import?> GetByIdAsync(int importId)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ProductVariant)
                        .ThenInclude(v => v.Color)
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ProductVariant)
                        .ThenInclude(v => v.Size)
                .Include(i => i.ImportDetails)
                    .ThenInclude(id => id.ImportStoreDetails)
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
            // 1. Chuẩn hóa PageNumber
            if (filter.PageNumber < 1)
                filter.PageNumber = 1;

            // 2. Build query
            var query = _context.Imports
                .Include(ii => ii.CreatedByNavigation)
                .Include(ii => ii.ImportDetails)
                    .ThenInclude(detail => detail.ImportStoreDetails)
                .AsQueryable();

            // 3. Áp dụng filter
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

            // 4. Đếm tổng bản ghi
            var totalCount = await query.CountAsync();

            // 5. Tính tổng số trang và kiểm tra
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            if (totalCount > 0 && filter.PageNumber > totalPages)
            {
                return new PagedResult<Import>
                {
                    Data = new List<Import>(),
                    TotalCount = totalCount
                };
            }

            // 6. Sắp xếp
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
                    query = query.OrderBy(ii => ii.ImportId);
                    break;
            }

            // 7. Phân trang
            var data = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // 8. Trả về kết quả, đảm bảo Data không null
            return new PagedResult<Import>
            {
                Data = data ?? new List<Import>(),
                TotalCount = totalCount
            };
        }

        public async Task<Import> GetImportByIdAsync(int importId)
        {
            return await _context.Imports
                .Include(i => i.CreatedByNavigation)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(pv => pv.Product) // Lấy ProductName
                                                       // Nếu Size và Color là navigation properties, include thêm chúng:
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(pv => pv.Size)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(pv => pv.Color)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                        .ThenInclude(s => s.StaffDetail)
                            .ThenInclude(sd => sd.Account)  // Include bảng Account
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                        .ThenInclude(s => s.Warehouse)
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }


        public async Task<Import?> GetByIdAssignAsync(int importId)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(detail => detail.ImportStoreDetails)
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        public void Add(Import import)
        {
            _context.Imports.Add(import);
        }

        public async Task<Import> GetByIdAsyncWithDetails(int id)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                .FirstOrDefaultAsync(i => i.ImportId == id);
        }

        public async Task<PaginatedResponseDTO<Warehouse>> GetAllWarehousesAsync(int page, int pageSize)
        {
            var query = _context.Warehouses
            
                .AsQueryable();

            int totalRecords = await query.CountAsync();

            var data = await query.OrderBy(w => w.WarehouseId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<Warehouse>(data, totalRecords, page, pageSize);
        }
        public async Task<Warehouse> GetWareHouseByIdAsync(int id)
        {
            return await _context.Warehouses.FindAsync(id);
        }
        public async Task<ImportStoreDetail> GetImportStoreDetail(int importId)
        {
            var data = await _context.ImportStoreDetails
                .Include(od => od.StaffDetail).ThenInclude(oc => oc.Account)
                .Include(od => od.Warehouse)
                .Include(od => od.HandleByNavigation)
                .Include(od => od.ImportDetail).ThenInclude(oc => oc.ProductVariant).ThenInclude(ot => ot.Product)
                .Include(od => od.ImportDetail)
                        .ThenInclude(c => c.Import)
                .FirstOrDefaultAsync(o => o.ImportStoreId == importId);
            return data;
        }

        public async Task<decimal?> GetLatestCostPriceAsync(int productId, int sizeId, int colorId)
        {
            var variantId = await _variantRepo.GetVariantIdAsync(productId, sizeId, colorId);
            if (variantId == null)
                return null;

            return await _context.ImportDetails
                .Where(d => d.ProductVariantId == variantId)
                .OrderByDescending(d => d.Import!.CompletedDate)
                .Select(d => (decimal?)d.CostPrice)
                .FirstOrDefaultAsync();
        }

    }
}
