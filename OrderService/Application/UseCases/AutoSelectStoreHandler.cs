using Application.DTO.Request;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AutoSelectStoreHandler
    {
        private readonly IInventoryServiceClient _inventoryServiceClient;

        public AutoSelectStoreHandler(IInventoryServiceClient inventoryServiceClient)
        {
            _inventoryServiceClient = inventoryServiceClient;
        }

        public async Task<int> AutoSelectStoreAsync(List<OrderItemRequest> orderItems, string city, string district)
        {
            var allStores = await _inventoryServiceClient.GetAllStoresAsync();
            var storeCoverageList = new List<StoreCoverage>();

            foreach (var store in allStores)
            {
                double coverageRatio = await CalculateCoverageRatio(store, orderItems);
                int totalStock = await CalculateTotalStock(store, orderItems);

                storeCoverageList.Add(new StoreCoverage
                {
                    StoreId = store.StoreId,
                    CoverageRatio = coverageRatio,
                    StockQuantity = totalStock
                });
            }

            return SelectBestStore(storeCoverageList);
        }

        /// <summary>
        /// Tính tỷ lệ số sản phẩm có đủ hàng tại cửa hàng này.
        /// </summary>
        private async Task<double> CalculateCoverageRatio(Store store, List<OrderItemRequest> orderItems)
        {
            int fulfillCount = 0;
            foreach (var item in orderItems)
            {
                int availableQuantity = await _inventoryServiceClient.GetStockQuantityAsync(store.StoreId, item.ProductVariantId);
                if (availableQuantity >= item.Quantity)
                {
                    fulfillCount++;
                }
            }
            return (double)fulfillCount / orderItems.Count;
        }

        /// <summary>
        /// Tính tổng số lượng hàng tồn kho của tất cả sản phẩm khách đặt tại cửa hàng.
        /// </summary>
        private async Task<int> CalculateTotalStock(Store store, List<OrderItemRequest> orderItems)
        {
            int totalStock = 0;
            foreach (var item in orderItems)
            {
                totalStock += await _inventoryServiceClient.GetStockQuantityAsync(store.StoreId, item.ProductVariantId);
            }
            return totalStock;
        }

        /// <summary>
        /// Chọn cửa hàng tốt nhất dựa trên số lượng hàng tồn kho.
        /// </summary>
        private int SelectBestStore(List<StoreCoverage> storeCoverageList)
        {
            // Chọn store có coverage ≥ 70%
            var bestCoverageStore = storeCoverageList
                .Where(x => x.CoverageRatio >= 0.7)
                .OrderByDescending(x => x.CoverageRatio)
                .FirstOrDefault();

            if (bestCoverageStore != null)
                return bestCoverageStore.StoreId;

            // Nếu không có store nào có đủ hàng ≥ 70%, chọn store có tổng số hàng tồn nhiều nhất
            var maxStockStore = storeCoverageList.OrderByDescending(x => x.StockQuantity).FirstOrDefault();
            if (maxStockStore != null)
                return maxStockStore.StoreId;

            // Nếu vẫn không có store nào, chọn random
            return storeCoverageList.OrderBy(x => Guid.NewGuid()).First().StoreId;
        }

        private record StoreCoverage
        {
            public int StoreId { get; init; }
            public double CoverageRatio { get; init; }
            public int StockQuantity { get; init; }
        }
    }

}
