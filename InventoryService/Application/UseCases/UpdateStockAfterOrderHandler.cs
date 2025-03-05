using Application.DTO.Request;
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
        /// <summary>
        /// Xử lý cập nhật tồn kho sau khi đặt hàng.
        /// Handler nhận vào StockUpdateRequest, chuyển đổi danh sách StockItemResponse thành danh sách tuple (VariantId, Quantity)
        /// và gọi repository để cập nhật tồn kho.
        /// Trả về true nếu cập nhật thành công, ngược lại trả về false.
        /// </summary>
        /// <param name="request">Đối tượng StockUpdateRequest chứa StoreId và danh sách các mục cần cập nhật</param>
        /// <returns>Boolean cho biết cập nhật tồn kho thành công hay không</returns>
        public async Task<bool> HandleAsync(StockUpdateRequest request)
        {
            // Chuyển đổi danh sách StockItemResponse thành danh sách tuple (VariantId, Quantity)
            var stockUpdates = request.Items
                                      .Select(i => (VariantId: i.VariantId, Quantity: i.Quantity))
                                      .ToList();

            // Gọi repository để cập nhật tồn kho trong DB
            bool success = await _storeStockRepository.UpdateStockAfterOrderAsync(request.StoreId, stockUpdates);
            return success;
        }
    }
}

