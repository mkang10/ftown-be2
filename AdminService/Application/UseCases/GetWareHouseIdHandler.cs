using Domain.DTOs;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.GetWareHouseIdHandler;

namespace Application.UseCases
{
   
        public class GetWareHouseIdHandler 
        {
            private readonly IWarehouseStockRepos _repository;

            public GetWareHouseIdHandler(IWarehouseStockRepos repository)
            {
                _repository = repository;
            }

            public async Task<WarehouseStockDto?> GetByIdAsync(int id)
            {
                var entity = await _repository.GetByIdWithDetailsAsync(id);
                if (entity == null) return null;

                var dto = new WarehouseStockDto
                {
                    WareHouseStockId = entity.WareHouseStockId,
                    VariantId = entity.VariantId,
                    VariantName = entity.Variant.Product.Name + " - " + entity.Variant.Color.ColorName + " - " + entity.Variant.Size.SizeName,
                    StockQuantity = entity.StockQuantity,
                    WareHouseId = entity.WareHouseId,
                    WareHouseName = entity.WareHouse.WarehouseName,
                    AuditHistory = entity.WareHouseStockAudits
                        .Select(a => new WarehouseStockAuditDto
                        {
                            AuditId = a.AuditId,
                            Action = a.Action,
                            QuantityChange = a.QuantityChange,
                            ActionDate = a.ActionDate,
                            ChangedBy = a.ChangedBy,
                            Note = a.Note
                        })
                        .OrderByDescending(a => a.ActionDate)
                        .ToList()
                };

                return dto;
            }
        }
    }

