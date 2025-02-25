using Application.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetStoreStockByVariantHandler
    {
        private readonly IStoreStockRepository _storeStockRepository;

        public GetStoreStockByVariantHandler(IStoreStockRepository storeStockRepository)
        {
            _storeStockRepository = storeStockRepository;
        }

        /// <summary>
        /// Lấy tổng tồn kho của một variant trên toàn hệ thống.
        /// </summary>
        public async Task<int> HandleTotalStockAsync(int variantId)
        {
            return await _storeStockRepository.GetTotalStockByVariantAsync(variantId);
        }

        /// <summary>
        /// Lấy phân rã tồn kho của một variant theo từng cửa hàng.
        /// </summary>
        public async Task<List<StoreStockResponse>> HandleStockBreakdownAsync(int variantId)
        {
            var storeStocks = await _storeStockRepository.GetStoreStocksByVariantAsync(variantId);
            var result = storeStocks.Select(ss => new StoreStockResponse
            {
                StoreId = ss.StoreId,
                StoreName = ss.Store.StoreName,
                StockQuantity = ss.StockQuantity
            }).ToList();

            return result;
        }
    }
}
