﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using static Domain.DTO.Request.StoreExportStoreDetailReq;

namespace Infrastructure
{
    public class DispatchRepos : IDispatchRepos
    {
        private readonly FtownContext _context;
        private readonly IMapper _mapper;

        public DispatchRepos(IMapper mapper, FtownContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy Dispatch với các DispatchDetail và StoreExportStoreDetails đã include
        public async Task<Dispatch?> GetByIdAssignAsync(int dispatchId)
        {
            return await _context.Dispatches
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                .FirstOrDefaultAsync(d => d.DispatchId == dispatchId);
        }

        // Lấy Dispatch theo Id (không include các navigation property nếu không cần thiết)
        public async Task<Dispatch?> GetByIdAsync(int dispatchId)
        {
            return await _context.Dispatches.FindAsync(dispatchId);
        }

        // Lấy danh sách Dispatch có cùng OriginalDispatchId (giả sử bạn có thuộc tính này trong Dispatch)
        public async Task<List<Dispatch>> GetAllByOriginalDispatchIdAsync(int originalDispatchId)
        {
            return await _context.Dispatches
                .Where(d => d.OriginalId == originalDispatchId)
                .ToListAsync();
        }

        // Reload lại entity từ DbContext
        public async Task ReloadAsync(Dispatch dispatch)
        {
            await _context.Entry(dispatch).ReloadAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponseDTO<DispatchResponseDto>> GetAllDispatchAsync(int page, int pageSize, DispatchFilterDto filter)
        {
            var query = _context.Dispatches
                .Include(d => d.CreatedByNavigation)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.Variant)
                        .ThenInclude(v => v.Product)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                        .ThenInclude(s => s.Warehouse)
                .Include(d => d.DispatchDetails)
                    .ThenInclude(dd => dd.StoreExportStoreDetails)
                        .ThenInclude(s => s.StaffDetail)
                            .ThenInclude(sd => sd.Account)
                .AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(d => d.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.ReferenceNumber))
                query = query.Where(d => d.ReferenceNumber!.Contains(filter.ReferenceNumber));

            if (filter.FromDate.HasValue)
                query = query.Where(d => d.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(d => d.CreatedDate <= filter.ToDate.Value);

            if (filter.WarehouseId.HasValue)
                query = query.Where(d => d.DispatchDetails.Any(dd =>
                    dd.StoreExportStoreDetails.Any(sd => sd.WarehouseId == filter.WarehouseId)));

            if (filter.StaffDetailId.HasValue)
                query = query.Where(d => d.DispatchDetails.Any(dd =>
                    dd.StoreExportStoreDetails.Any(sd => sd.StaffDetailId == filter.StaffDetailId)));

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(d => d.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<DispatchResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PaginatedResponseDTO<DispatchResponseDto>(data, total, page, pageSize);
        }

        public async Task<Dispatch?> GetByIdDispatchAssignAsync(int dispatchId)
        {
            return await _context.Dispatches
                .Include(i => i.DispatchDetails)
                    .ThenInclude(detail => detail.StoreExportStoreDetails)
                .FirstOrDefaultAsync(i => i.DispatchId == dispatchId);
        }
        public async Task UpdateAsync(Dispatch dispatch)
        {
            _context.Dispatches.Update(dispatch);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponseDTO<ExportDetailDto>> GetAllExportStoreDetailsAsync(
    int page,
    int pageSize,
    StoreExportStoreDetailFilterDto filter)
        {
            var query = _context.StoreExportStoreDetails
                .AsNoTracking()
                .Include(e => e.Warehouse)
                .Include(e => e.StaffDetail)
                    .ThenInclude(sd => sd.Account)
                .Include(e => e.DispatchDetail) // Đảm bảo include DispatchDetail
                .AsQueryable();

            // --- Filters ---
            if (filter.DispatchDetailId.HasValue)
                query = query.Where(e => e.DispatchDetailId == filter.DispatchDetailId.Value);

            if (filter.WarehouseId.HasValue)
                query = query.Where(e => e.WarehouseId == filter.WarehouseId.Value);

            if (filter.StaffDetailId.HasValue)
                query = query.Where(e => e.StaffDetailId == filter.StaffDetailId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var st = filter.Status.Trim().ToLower();
                query = query.Where(e => e.Status != null && e.Status.ToLower().Contains(st));
            }

            if (!string.IsNullOrWhiteSpace(filter.Comments))
            {
                var cm = filter.Comments.Trim().ToLower();
                query = query.Where(e => e.Comments != null && e.Comments.ToLower().Contains(cm));
            }

            // --- Sorting ---
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                bool desc = filter.IsDescending;
                switch (filter.SortBy.Trim().ToLower())
                {
                    case "warehouseid":
                        query = desc
                            ? query.OrderByDescending(e => e.WarehouseId)
                            : query.OrderBy(e => e.WarehouseId);
                        break;
                    case "warehousename":
                        query = desc
                            ? query.OrderByDescending(e => e.Warehouse.WarehouseName)
                            : query.OrderBy(e => e.Warehouse.WarehouseName);
                        break;
                    case "allocatedquantity":
                        query = desc
                            ? query.OrderByDescending(e => e.AllocatedQuantity)
                            : query.OrderBy(e => e.AllocatedQuantity);
                        break;
                    case "status":
                        query = desc
                            ? query.OrderByDescending(e => e.Status)
                            : query.OrderBy(e => e.Status);
                        break;
                    case "comments":
                        query = desc
                            ? query.OrderByDescending(e => e.Comments)
                            : query.OrderBy(e => e.Comments);
                        break;
                    case "staffname":
                        query = desc
                            ? query.OrderByDescending(e => e.StaffDetail != null
                                ? e.StaffDetail.Account.FullName
                                : string.Empty)
                            : query.OrderBy(e => e.StaffDetail != null
                                ? e.StaffDetail.Account.FullName
                                : string.Empty);
                        break;
                    default:
                        query = query.OrderBy(e => e.DispatchStoreDetailId);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(e => e.DispatchStoreDetailId);
            }

            // --- Paging ---
            var totalCount = await query.CountAsync();
            var items = await query
                .ProjectTo<ExportDetailDto>(_mapper.ConfigurationProvider)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<ExportDetailDto>(items, totalCount, page, pageSize);
        }
    }
  }