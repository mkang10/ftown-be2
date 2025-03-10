﻿using Application.DTO.Request;
using Application.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class UpdateStockAfterOrderHandler
    {
        private readonly IStoreStockRepository _storeStockRepository;
        public UpdateStockAfterOrderHandler(IStoreStockRepository storeStockRepository)
        {
            _storeStockRepository = storeStockRepository;
        }
        public async Task<StockUpdateResponse> HandleAsync(StockUpdateRequest request)
        {
            // Chuyển đổi danh sách StockItemResponse thành danh sách tuple (VariantId, Quantity)
            var stockUpdates = request.Items
                                      .Select(i => (VariantId: i.VariantId, Quantity: i.Quantity))
                                      .ToList();

            // Gọi repository để cập nhật tồn kho trong DB
            bool success = await _storeStockRepository.UpdateStockAfterOrderAsync(request.StoreId, stockUpdates);

            // Trả về response
            return new StockUpdateResponse
            {
                Success = success,
                Message = success ? "Cập nhật tồn kho thành công." : "Cập nhật tồn kho thất bại.",
            };
        }
    }
}

