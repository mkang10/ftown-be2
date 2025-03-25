using Application.DTO.Request;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response.Application.Imports.Dto;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateImportHandler
    {
        private readonly IImportRepos _impRepos;
        private readonly IMapper _mapper;
        private readonly IAuditLogRepos _auditLogRepos;


        public CreateImportHandler(IImportRepos impRepos, IMapper mapper, IAuditLogRepos auditLogRepos)
        {
            _impRepos = impRepos;
            _mapper = mapper;
            _auditLogRepos = auditLogRepos;
        }
        public async Task<ResponseDTO<ImportDto>> CreateImportAsync(CreateImportDto request)
        {
            // Sinh tự động ReferenceNumber nếu không hợp lệ
            if (string.IsNullOrEmpty(request.ReferenceNumber) || !request.ReferenceNumber.StartsWith("IIN"))
            {
                Random rnd = new Random();
                request.ReferenceNumber = "IIN" + rnd.Next(100, 1000).ToString();
            }

            // Tính tổng tiền từ các ImportDetail (Quantity * UnitPrice)
            decimal totalCost = request.ImportDetails.Sum(d => d.Quantity * d.UnitPrice);

            // Map từ Request DTO sang Entity
            var newImport = _mapper.Map<Import>(request);

            // Set các trường bổ sung không được mapping tự động
            newImport.CreatedDate = DateTime.UtcNow;
            newImport.Status = "Pending";
            newImport.TotalCost = totalCost;
            newImport.ApprovedDate = null;
            newImport.CompletedDate = null;

            // Lưu Import qua repository
            _impRepos.Add(newImport);
            await _impRepos.SaveChangesAsync();

            // Tạo AuditLog
            // Ở đây, thay vì serialize toàn bộ entity (có thể gây vòng lặp),
            // bạn đã map entity sang DTO (với mapping profile loại bỏ vòng lặp)
            // và serialize DTO đó.
            var serializedChangeData = JsonConvert.SerializeObject(newImport,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var auditLog = new AuditLog
            {
                TableName = "Import",
                RecordId = newImport.ImportId.ToString(),
                Operation = "CREATE",
                ChangeDate = DateTime.UtcNow,
                ChangedBy = request.CreatedBy,
                ChangeData = serializedChangeData,
                Comment = "Tạo mới đơn import"
            };

            _auditLogRepos.Add(auditLog);
            await _auditLogRepos.SaveChangesAsync();

            // Map từ Entity sang Response DTO
            var resultDto = _mapper.Map<ImportDto>(newImport);
            return new ResponseDTO<ImportDto>(resultDto, true, "Tạo import thành công!");
        }
    }
}




