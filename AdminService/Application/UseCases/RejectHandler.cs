using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{

public class RejectHandler    {
        private readonly IInventoryImportRepository _repository;
        private readonly IMapper _mapper;

        public RejectHandler(IInventoryImportRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

       
        // Hàm reject đơn import
        public async Task RejectImportAsync(int importId, int changedBy, string? comments)
        {
            // Lấy đơn import theo ID
            var import = await _repository.GetByIdAsync(importId);
            if (import == null)
                throw new Exception("Inventory import not found.");

            // Chỉ xử lý nếu trạng thái hiện tại là "pending"
            if (!string.Equals(import.Status?.Trim(), "pending", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only pending import requests can be rejected.");

            // Cập nhật trạng thái và các trường liên quan
            import.Status = "rejected";
            import.CompletedDate = DateTime.UtcNow;

            // Tạo một bản ghi History mới
            var history = new InventoryImportHistory
            {
                ImportId = import.ImportId,
                Status = "rejected",
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                Comments = comments
            };

            // Lấy đối tượng Account từ repository
            var account = await _repository.GetAccountByIdAsync(changedBy);
            if (account == null)
                throw new Exception($"Account with ID = {changedBy} not found.");

            history.ChangedByNavigation = account;

            // Thêm history vào đơn import
            import.InventoryImportHistories.Add(history);

            // Cập nhật lại đơn import
            await _repository.UpdateAsync(import);
        }

    }

}

