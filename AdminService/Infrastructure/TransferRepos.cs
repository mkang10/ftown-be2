﻿using Domain.DTO.Response;
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
    public class TransferRepos : ITransferRepos
    {
        private readonly FtownContext _context;
        public TransferRepos(FtownContext context)
        {
            _context = context;
        }

        public void Add(Transfer transfer)
        {
            _context.Transfers.Add(transfer);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponseDTO<TransferDto>> GetAllWithPagingAsync(
    int page,
    int pageSize,
    string? filter,
    CancellationToken cancellationToken = default)
        {
            var query = _context.Transfers
                .AsNoTracking()
                .Select(t => new TransferDto
                {
                    
                    TransferOrderId = t.TransferOrderId,
                    ImportId = t.ImportId,
                    ImportReferenceNumber = t.Import.ReferenceNumber,
                    DispatchId = t.DispatchId,
                    DispatchReferenceNumber = t.Dispatch.ReferenceNumber,
                    CreatedBy = t.CreatedBy,
                    CreatedByName = _context.Accounts
                                                .Where(a => a.AccountId == t.CreatedBy)
                                                .Select(a => a.FullName)
                                                .FirstOrDefault()!,
                    CreatedDate = t.CreatedDate,
                    Status = t.Status,
                    Remarks = t.Remarks,
                    OriginalTransferOrderId = t.OriginalTransferOrderId
                });

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var norm = filter.Trim().ToLower();
                query = query.Where(t =>
                    t.Status.ToLower().Contains(norm) ||
                    (t.Remarks != null && t.Remarks.ToLower().Contains(norm)) ||
                    t.ImportReferenceNumber.ToLower().Contains(norm) ||
                    t.DispatchReferenceNumber.ToLower().Contains(norm) ||
                    t.CreatedByName.ToLower().Contains(norm)
                );
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDTO<TransferDto>(data, total, page, pageSize);
        }
    }


}