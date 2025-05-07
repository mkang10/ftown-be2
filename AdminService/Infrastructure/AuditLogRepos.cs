using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure;

public class AuditLogRepository : IAuditLogRepos
{
    private readonly FtownContext _context;

    public AuditLogRepository(FtownContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Thêm một bản ghi AuditLog vào hệ thống.
    /// </summary>
    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
    }

    /// <summary>
    /// Lưu thay đổi vào database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Lấy danh sách audit log theo bảng và ID bản ghi.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByRecordIdAsync(string tableName, string recordId)
    {
        return await _context.AuditLogs
            .Where(log => log.TableName == tableName && log.RecordId == recordId).Include(t => t.ChangedByNavigation)
            .OrderByDescending(log => log.ChangeDate)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách audit log theo user thực hiện.
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetByUserAsync(int changedBy)
    {
        return await _context.AuditLogs
            .Where(log => log.ChangedBy == changedBy)
            .OrderByDescending(log => log.ChangeDate)
            .ToListAsync();
    }

    public void Add(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
    }
    public async Task<List<AuditLog>> GetAuditLogsByTableAndRecordIdAsync(string tableName, string recordId)
    {
        return await _context.AuditLogs
            .Where(a => a.TableName == tableName && a.RecordId == recordId).Include(t => t.ChangedByNavigation)
            .ToListAsync();
    }
}
