﻿using Application.DTO.Request;
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
using static Application.UseCases.CreateImportHandler;

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



        public async Task<ResponseDTO<ImportDto>> CreateSupplementImportAsync(SupplementImportRequestDto request)
        {
            // Kiểm tra dữ liệu đầu vào
            if (request.OriginalImportId <= 0)
                throw new ArgumentException("OriginalImportId không hợp lệ.");
            if (request.ImportDetails == null || !request.ImportDetails.Any())
                throw new ArgumentException("Không có thông tin import detail.");

            // Lấy đơn cũ với eager loading các ImportDetails và ImportStoreDetails
            var oldImport = await _impRepos.GetByIdAsyncWithDetails(request.OriginalImportId);
            if (oldImport == null)
                throw new ArgumentException("Đơn cũ không tồn tại.");

            // Sử dụng Automapper để map các trường từ request sang Import (các trường cơ bản từ supplement DTO)
            var newImport = _mapper.Map<Import>(request);

            // Gán lại các trường cần thiết từ đơn cũ
            newImport.CreatedBy = oldImport.CreatedBy;
            newImport.ReferenceNumber = oldImport.ReferenceNumber;
            newImport.CreatedDate = DateTime.UtcNow;
            newImport.Status = "Pending";
            newImport.ApprovedDate = null;
            newImport.CompletedDate = null;
            newImport.OriginalImportId = oldImport.ImportId;

            decimal totalCost = 0;
            newImport.ImportDetails = new List<ImportDetail>();

            // Duyệt qua từng ImportDetail của đơn cũ để tạo mới ImportDetail và ImportStoreDetails
            foreach (var oldDetail in oldImport.ImportDetails)
            {
                // Tìm thông tin unitPrice từ request theo ProductVariantId
                var reqDetail = request.ImportDetails.FirstOrDefault(d => d.ProductVariantId == oldDetail.ProductVariantId);
                if (reqDetail == null)
                    throw new ArgumentException($"Thiếu thông tin unitPrice cho sản phẩm có ProductVariantId {oldDetail.ProductVariantId}");

                int totalMissing = 0;
                var newDetail = new ImportDetail
                {
                    ProductVariantId = oldDetail.ProductVariantId,
                    Quantity = 0, // Sẽ cập nhật sau khi tính tổng số lượng thiếu
                    ImportStoreDetails = new List<ImportStoreDetail>()
                };

                // Duyệt qua các ImportStoreDetail của chi tiết đơn cũ
                if (oldDetail.ImportStoreDetails != null && oldDetail.ImportStoreDetails.Any())
                {
                    foreach (var store in oldDetail.ImportStoreDetails)
                    {
                        int actualReceived = store.ActualReceivedQuantity ?? 0;
                        int missing = store.AllocatedQuantity - actualReceived;
                        if (missing > 0)
                        {
                            totalMissing += missing;
                            newDetail.ImportStoreDetails.Add(new ImportStoreDetail
                            {
                                ActualReceivedQuantity = null, // để null
                                AllocatedQuantity = missing,     // bằng số lượng thiếu
                                Status = "Pending",              // trạng thái Pending
                                StaffDetailId = store.StaffDetailId, // copy từ store cũ
                                WarehouseId = store.WarehouseId,   // giữ nguyên
                                HandleBy = store.HandleBy
                            });
                        }
                    }
                }
                newDetail.Quantity = totalMissing;
                if (totalMissing > 0)
                {
                    newImport.ImportDetails.Add(newDetail);
                    totalCost += totalMissing * reqDetail.UnitPrice;
                }
            }

            newImport.TotalCost = totalCost;

            // Lưu đơn bổ sung mới qua repository
            _impRepos.Add(newImport);
            await _impRepos.SaveChangesAsync();

            // Tạo AuditLog cho đơn mới
            var serializedNewImport = JsonConvert.SerializeObject(newImport,
                 new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var auditLogNew = new AuditLog
            {
                TableName = "Import",
                RecordId = newImport.ImportId.ToString(),
                Operation = "CREATE",
                ChangeDate = DateTime.UtcNow,
                ChangedBy = oldImport.CreatedBy,
                ChangeData = serializedNewImport,
                Comment = "Tạo mới đơn import bổ sung dựa trên đơn cũ"
            };
            _auditLogRepos.Add(auditLogNew);
            await _auditLogRepos.SaveChangesAsync();

            // Cập nhật trạng thái cho đơn cũ
            oldImport.Status = "Supplement Created";
            await _impRepos.SaveChangesAsync();

            var serializedOldImport = JsonConvert.SerializeObject(oldImport,
                 new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var auditLogOld = new AuditLog
            {
                TableName = "Import",
                RecordId = oldImport.ImportId.ToString(),
                Operation = "UPDATE",
                ChangeDate = DateTime.UtcNow,
                ChangedBy = oldImport.CreatedBy,
                ChangeData = serializedOldImport,
                Comment = "Cập nhật đơn cũ với status 'Supplement Created'"
            };
            _auditLogRepos.Add(auditLogOld);
            await _auditLogRepos.SaveChangesAsync();

            // Map entity sang DTO response
            var resultDto = _mapper.Map<ImportDto>(newImport);
            return new ResponseDTO<ImportDto>(resultDto, true, "Tạo đơn import bổ sung thành công!");
        }

    }



}





