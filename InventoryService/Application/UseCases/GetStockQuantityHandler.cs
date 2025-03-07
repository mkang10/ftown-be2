using Application.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetStockQuantityHandler
    {
        private readonly IStoreStockRepository _storeStockRepository;

        public GetStockQuantityHandler(IStoreStockRepository storeStockRepository)
        {
            _storeStockRepository = storeStockRepository;
        }

        public async Task<StockQuantityResponse> HandleAsync(int storeId, int productVariantId)
        {
            int stockQuantity = await _storeStockRepository.GetStockQuantityAsync(storeId, productVariantId);

            return new StockQuantityResponse
            {
                StoreId = storeId,
                ProductVariantId = productVariantId,
                StockQuantity = stockQuantity
            };
        }
    }
}
