using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuditLogRepos
    {
        /// <summary>
        /// Thêm một bản ghi AuditLog vào hệ thống.
        /// </summary>
        Task AddAsync(AuditLog auditLog);

        /// <summary>
        /// Lưu thay đổi vào database.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Lấy danh sách audit log theo bảng và ID bản ghi.
        /// </summary>
        Task<IEnumerable<AuditLog>> GetByRecordIdAsync(string tableName, string recordId);

        /// <summary>
        /// Lấy danh sách audit log theo user thực hiện.
        /// </summary>
        Task<IEnumerable<AuditLog>> GetByUserAsync(int changedBy);
    }
}
