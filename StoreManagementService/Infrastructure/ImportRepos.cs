using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Application.Imports.Dto;
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

        public async Task<StaffDetail?> GetStaffDetailByIdAsync(int staffDetailId)
        {
            // AsNoTracking() cho trường hợp chỉ đọc dữ liệu
            return await _context.StaffDetails
                                   .AsNoTracking()
                                    .Include(s => s.Account)
                                   .FirstOrDefaultAsync(s => s.StaffDetailId == staffDetailId);
        }


        public async Task<PaginatedResponseDTO<ImportStoreDetailDto>> GetImportStoreDetailByStaffDetailAsync(ImportStoreDetailFilterDtO filter)
        {
            // 1. Build base query (AsNoTracking + filter HandleBy + optional Status)
            var baseQuery = _context.ImportStoreDetails
                .AsNoTracking()
                .Where(x => x.HandleBy == filter.HandleBy);

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var pattern = $"%{filter.Status}%";
                baseQuery = baseQuery.Where(x => EF.Functions.Like(x.Status, pattern));
            }

            // 2. Count trên baseQuery (không join)
            var total = await baseQuery.CountAsync();

            // 3. Apply sorting
            bool desc = filter.IsDescending;
            IQueryable<ImportStoreDetail> orderedQuery = filter.SortBy?.Trim().ToLower() switch
            {
                "actualreceivedquantity" => desc
                    ? baseQuery.OrderByDescending(x => x.ActualReceivedQuantity)
                    : baseQuery.OrderBy(x => x.ActualReceivedQuantity),

                "allocatedquantity" => desc
                    ? baseQuery.OrderByDescending(x => x.AllocatedQuantity)
                    : baseQuery.OrderBy(x => x.AllocatedQuantity),

                "status" => desc
                    ? baseQuery.OrderByDescending(x => x.Status)
                    : baseQuery.OrderBy(x => x.Status),

                "comments" => desc
                    ? baseQuery.OrderByDescending(x => x.Comments)
                    : baseQuery.OrderBy(x => x.Comments),

                "warehouseid" => desc
                    ? baseQuery.OrderByDescending(x => x.WarehouseId)
                    : baseQuery.OrderBy(x => x.WarehouseId),

                "handleby" => desc
                    ? baseQuery.OrderByDescending(x => x.HandleBy)
                    : baseQuery.OrderBy(x => x.HandleBy),

                "productname" => desc
                    ? baseQuery.OrderByDescending(x => x.ImportDetail.ProductVariant.Product.Name)
                    : baseQuery.OrderBy(x => x.ImportDetail.ProductVariant.Product.Name),

                "sizename" => desc
                    ? baseQuery.OrderByDescending(x => x.ImportDetail.ProductVariant.Size.SizeName)
                    : baseQuery.OrderBy(x => x.ImportDetail.ProductVariant.Size.SizeName),

                "colorname" => desc
                    ? baseQuery.OrderByDescending(x => x.ImportDetail.ProductVariant.Color.ColorName)
                    : baseQuery.OrderBy(x => x.ImportDetail.ProductVariant.Color.ColorName),

                _ => desc
                    ? baseQuery.OrderByDescending(x => x.ImportDetailId)
                    : baseQuery.OrderBy(x => x.ImportDetailId),
            };

            // 4. Paging
            var pagedQuery = orderedQuery
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);

            // 5. Projection
            var items = await pagedQuery
                .Select(x => new ImportStoreDetailDto
                {
                    ImportId = x.ImportDetail.ImportId,
                    ImportDetailId = x.ImportDetailId,
                    ImportStoreId = x.ImportStoreId,
                    ActualReceivedQuantity = x.ActualReceivedQuantity,
                    AllocatedQuantity = x.AllocatedQuantity,
                    Status = x.Status,
                    Comments = x.Comments,
                    WareHouseId = (int)x.WarehouseId,
                    WarehouseName = x.Warehouse != null
                                                ? x.Warehouse.WarehouseName
                                                : null,
                    HandleBy = x.HandleBy,
                    HandleByName = x.HandleByNavigation.Account.FullName,
                    ProductName = x.ImportDetail.ProductVariant.Product.Name,
                    SizeName = x.ImportDetail.ProductVariant.Size.SizeName,
                    ColorName = x.ImportDetail.ProductVariant.Color.ColorName,
                })
                .ToListAsync();

            // 6. Trả về kết quả phân trang
            return new PaginatedResponseDTO<ImportStoreDetailDto>(
                items,
                total,
                filter.Page,
                filter.PageSize
            );
        }


        public async Task<(IEnumerable<Import>, int)> GetAllImportsAsync(ImportFilterDto filter, CancellationToken cancellationToken)
        {
            var query = _context.Imports
                .Include(i => i.CreatedByNavigation)
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(i => i.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.ReferenceNumber))
                query = query.Where(i => i.ReferenceNumber.Contains(filter.ReferenceNumber));

            if (filter.HandleBy.HasValue)
            {
                query = query.Where(i => i.ImportDetails.Any(d =>
                                  d.ImportStoreDetails.Any(s => s.HandleBy == filter.HandleBy.Value)));
            }

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
        public async Task<PaginatedResponseDTO<ProductVariant>> GetAllAsync(int page, int pageSize, string? search = null)
        {
            // Xây dựng query và chỉ lọc status = "Draft"
            var query = _context.ProductVariants
                .Include(pv => pv.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(pv => pv.Size)
                .Include(pv => pv.Color)
                .AsQueryable();

            // Nếu có từ khóa tìm kiếm, lọc theo tên sản phẩm, màu sắc, kích thước
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();
                query = query.Where(pv =>
                    pv.Product.Name.ToLower().Contains(keyword) ||
                    pv.Color.ColorName.ToLower().Contains(keyword) ||
                    pv.Size.SizeName.ToLower().Contains(keyword) ||
                    pv.Sku.ToLower().Contains(keyword)
                );
            }

            // Tổng số bản ghi sau khi lọc
            int totalRecords = await query.CountAsync();

            // Áp dụng phân trang
            var data = await query
                .OrderBy(pv => pv.VariantId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<ProductVariant>(data, totalRecords, page, pageSize);
        }





        public async Task<Import?> GetByIdAssignAsync(int importId)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(detail => detail.ImportStoreDetails)
                .FirstOrDefaultAsync(i => i.ImportId == importId);
        }

        public async Task<Transfer> GetTransferByImportIdAsync(int importId)
        {
            // Lấy Transfer mà có trong collection Imports tồn tại Import có importId bằng giá trị truyền vào
            var transfer = await _context.Transfers
                .Include(t => t.Import)     
                .Include(t => t.Dispatch)   
        .FirstOrDefaultAsync(t => t.ImportId == importId);

            return transfer;
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

        public async Task<Import> GetByIdAsyncWithDetails(int id)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                .FirstOrDefaultAsync(i => i.ImportId == id);
        }

        public async Task<List<Import>> GetAllByOriginalImportIdAsync(int originalImportId)
        {
            return await _context.Imports
                .Include(i => i.ImportDetails)
                    .ThenInclude(d => d.ImportStoreDetails)
                .Where(i => i.OriginalImportId == originalImportId)
                .ToListAsync();
        }
        public async Task ReloadAsync(Import import)
        {
            // Reload đối tượng Import
            await _context.Entry(import).ReloadAsync();

            // Reload các ImportDetail
            foreach (var importDetail in import.ImportDetails)
            {
                await _context.Entry(importDetail).ReloadAsync();

                // Reload các ImportStoreDetail trong ImportDetail
                foreach (var storeDetail in importDetail.ImportStoreDetails)
                {
                    await _context.Entry(storeDetail).ReloadAsync();
                }
            }
        }




        public async Task<PaginatedResponseDTO<ImportStoreDetailDto>> GetStoreDetailsByStaffDetailAsync(
    ImportStoreDetailFilterDto filter)
        {
            // Base query: filter by staff
            var query = _context.ImportStoreDetails
                .AsNoTracking()
                .Where(s => s.StaffDetailId == filter.StaffDetailId);

            // Apply Status filter (case-insensitive using SQL LIKE)
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var statusPattern = $"%{filter.Status}%";
                query = query.Where(s => EF.Functions.Like(s.Status, statusPattern));
            }

            // Project into DTO with null-safe assignments matching DTO types
            var baseQuery = query.Select(s => new ImportStoreDetailDto
            {
                ImportId = s.ImportDetail.ImportId,
                ImportDetailId = s.ImportDetailId,
                WareHouseId = (int)s.WarehouseId,
                ActualReceivedQuantity = s.ActualReceivedQuantity,

                AllocatedQuantity = s.AllocatedQuantity,
                Status = s.Status,
                Comments = s.Comments,
                StaffDetailId = s.StaffDetailId,
                ImportStoreId = s.ImportStoreId,

                HandleBy = s.HandleBy,
                HandleByName = s.HandleByNavigation != null && s.HandleByNavigation.Account != null
                                           ? s.HandleByNavigation.Account.FullName
                                           : null,

                ProductName = s.ImportDetail.ProductVariant.Product.Name,
                SizeName = s.ImportDetail.ProductVariant.Size.SizeName,
                ColorName = s.ImportDetail.ProductVariant.Color.ColorName
            });

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                bool desc = filter.IsDescending;
                switch (filter.SortBy.Trim().ToLower())
                {
                    case "warehouseid":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.WareHouseId)
                            : baseQuery.OrderBy(dto => dto.WareHouseId);
                        break;
                    case "actualreceivedquantity":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.ActualReceivedQuantity)
                            : baseQuery.OrderBy(dto => dto.ActualReceivedQuantity);
                        break;
                    case "allocatedquantity":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.AllocatedQuantity)
                            : baseQuery.OrderBy(dto => dto.AllocatedQuantity);
                        break;
                    case "status":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.Status)
                            : baseQuery.OrderBy(dto => dto.Status);
                        break;
                    case "comments":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.Comments)
                            : baseQuery.OrderBy(dto => dto.Comments);
                        break;
                    case "staffdetailid":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.StaffDetailId)
                            : baseQuery.OrderBy(dto => dto.StaffDetailId);
                        break;
                    case "importstoreid":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.ImportStoreId)
                            : baseQuery.OrderBy(dto => dto.ImportStoreId);
                        break;
                    case "handleby":
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.HandleBy)
                            : baseQuery.OrderBy(dto => dto.HandleBy);
                        break;
                    default:
                        baseQuery = desc
                            ? baseQuery.OrderByDescending(dto => dto.ImportDetailId)
                            : baseQuery.OrderBy(dto => dto.ImportDetailId);
                        break;
                }
            }
            else
            {
                // Default sort by ImportDetailId
                baseQuery = baseQuery.OrderBy(dto => dto.ImportDetailId);
            }

            // Get total count before pagination
            var totalCount = await baseQuery.CountAsync();

            // Apply pagination and fetch data
            var items = await baseQuery
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Return paginated response
            return new PaginatedResponseDTO<ImportStoreDetailDto>(items, totalCount, filter.Page, filter.PageSize);
        }


        /// <summary>
        /// Trả về một IQueryable để truy vấn tất cả ImportDetail, bao gồm navigation Import.
        /// </summary>
        public IQueryable<ImportDetail> QueryImportDetails()
        {
            return _context.ImportDetails
                .AsNoTracking()
                .Include(d => d.Import);
        }

        /// <summary>
        /// Kiểm tra xem Import có phát sinh từ Transfer hay không.
        /// Xem trong bảng TransferOrder.
        /// </summary>
        public async Task<bool> HasTransferForImportAsync(int importId)
        {
            return await _context.Transfers
                .AsNoTracking()
                .AnyAsync(t => t.ImportId == importId);
        }

        /// <summary>
        /// Lấy ProductVariant theo ID, ném ngoại lệ nếu không tồn tại.
        /// </summary>
        public async Task<ProductVariant> GetProductVariantByIdAsync(int variantId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null)
                throw new KeyNotFoundException($"ProductVariant with ID {variantId} was not found.");

            return variant;
        }

    }
}