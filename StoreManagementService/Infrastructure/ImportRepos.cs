using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ImportRepos : IImportRepos
    {
        private readonly FtownContext _context;

        public ImportRepos(FtownContext context)
        {
            _context = context;
        }

        public async Task<Import> AddAsync(Import inventoryImport)
        {
            await _context.Imports.AddAsync(inventoryImport);
            await _context.SaveChangesAsync();
            return inventoryImport;
        }

        public async Task<(IEnumerable<Import>, int)> GetAllImportsAsync(ImportFilterDto filter, CancellationToken cancellationToken)
        {
            var query = _context.Imports
                .Include(i => i.CreatedByNavigation)
                .Include(i => i.HandleByNavigation) // Lấy thông tin ShopManagerDetail
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(i => i.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.ReferenceNumber))
                query = query.Where(i => i.ReferenceNumber.Contains(filter.ReferenceNumber));

            if (filter.HandleBy.HasValue)
                query = query.Where(i => i.HandleBy == filter.HandleBy);

            if (filter.FromDate.HasValue)
                query = query.Where(i => i.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(i => i.CreatedDate <= filter.ToDate.Value);

            // Sorting
            query = filter.SortBy switch
            {
                "ReferenceNumber" => filter.IsDescending ? query.OrderByDescending(i => i.ReferenceNumber) : query.OrderBy(i => i.ReferenceNumber),
                "CreatedDate" => filter.IsDescending ? query.OrderByDescending(i => i.CreatedDate) : query.OrderBy(i => i.CreatedDate),
                _ => filter.IsDescending ? query.OrderByDescending(i => i.ImportId) : query.OrderBy(i => i.ImportId),
            };

            int totalRecords = await query.CountAsync(cancellationToken);

            // Phân trang
            var imports = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return (imports, totalRecords);
        }
        public async Task<List<ProductVariant>> GetAllAsync()
        {
            return await _context.ProductVariants
                .Include(pv => pv.Product) // Include để lấy Product.Name
                .ToListAsync();
        }
        public async Task<Import?> GetByIdAssignAsync(int importId)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(detail => detail.ImportStoreDetails)
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }
        public async Task UpdateAsync(Import import)
        {
            _context.Imports.Update(import);
            await _context.SaveChangesAsync();
        }

        public void Add(Import import)
        {
            _context.Imports.Add(import);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Import?> GetByIdAsync(int importId)
        {
            return await _context.Imports

                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }

        public async Task<PaginatedResponseDTO<ImportStoreDetail>> GetStoreDetailsByStaffDetailAsync(ImportStoreDetailFilterDto filter)
        {
            var query = _context.ImportStoreDetails
                .Include(s => s.Warehouse)
                .Include(s => s.StaffDetail)
                .Include(s => s.ImportDetail)
                    .ThenInclude(d => d.Import) // Thêm Include để lấy ImportId
                .Where(s => s.StaffDetailId == filter.StaffDetailId)
                .AsQueryable();

            // Áp dụng filter theo Status nếu có
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(s => s.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            // Sắp xếp theo trường được chỉ định
            switch (filter.SortBy?.ToLower())
            {
                case "importstoreid":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.ImportStoreId)
                        : query.OrderBy(s => s.ImportStoreId);
                    break;
                case "importdetailid":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.ImportDetailId)
                        : query.OrderBy(s => s.ImportDetailId);
                    break;
                case "warehouseid":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.WarehouseId)
                        : query.OrderBy(s => s.WarehouseId);
                    break;
                case "allocatedquantity":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.AllocatedQuantity)
                        : query.OrderBy(s => s.AllocatedQuantity);
                    break;
                case "status":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.Status)
                        : query.OrderBy(s => s.Status);
                    break;
                case "comments":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.Comments)
                        : query.OrderBy(s => s.Comments);
                    break;
                case "staffdetailid":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.StaffDetailId)
                        : query.OrderBy(s => s.StaffDetailId);
                    break;
                case "storeimportstoreid":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.ImportStoreId) // Giả sử trường này dùng ImportStoreId
                        : query.OrderBy(s => s.ImportStoreId);
                    break;
                case "warehousename":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.Warehouse.WarehouseName)
                        : query.OrderBy(s => s.Warehouse.WarehouseName);
                    break;
                case "staffname":
                    query = filter.IsDescending
                        ? query.OrderByDescending(s => s.StaffDetail != null ? s.StaffDetail.Account.FullName : "")
                        : query.OrderBy(s => s.StaffDetail != null ? s.StaffDetail.Account.FullName : "");
                    break;
                default:
                    query = query.OrderBy(s => s.ImportStoreId);
                    break;
            }

            // Lấy tổng số bản ghi thỏa filter
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang
            var data = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<ImportStoreDetail>(data, totalCount, filter.Page, filter.PageSize);
        }

    }
}