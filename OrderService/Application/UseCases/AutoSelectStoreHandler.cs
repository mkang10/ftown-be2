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
                var fulfillmentData = await CalculateFulfillmentData(store, orderItems);

                storeCoverageList.Add(new StoreCoverage
                {
                    StoreId = store.StoreId,
                    FulfilledItemCount = fulfillmentData.FulfilledItemCount,
                    CoverageRatio = fulfillmentData.CoverageRatio,
                    StockQuantity = fulfillmentData.TotalStock
                });
            }

            return SelectBestStore(storeCoverageList);
        }

        /// <summary>
        /// Tính toán dữ liệu khả năng đáp ứng đơn hàng của cửa hàng
        /// </summary>
        private async Task<StoreFulfillmentData> CalculateFulfillmentData(Store store, List<OrderItemRequest> orderItems)
        {
            int fulfilledItemCount = 0;
            int fulfillCount = 0;
            int totalStock = 0;

            foreach (var item in orderItems)
            {
                int availableQuantity = await _inventoryServiceClient.GetStockQuantityAsync(store.StoreId, item.ProductVariantId);
                totalStock += availableQuantity;

                if (availableQuantity >= item.Quantity)
                {
                    fulfillCount++;
                    fulfilledItemCount += item.Quantity; // Tổng số sản phẩm có thể giao
                }
            }

            return new StoreFulfillmentData
            {
                FulfilledItemCount = fulfilledItemCount,
                CoverageRatio = (double)fulfillCount / orderItems.Count,
                TotalStock = totalStock
            };
        }

        /// <summary>
        /// Chọn cửa hàng tốt nhất dựa trên số lượng sản phẩm có thể giao.
        /// </summary>
        private int SelectBestStore(List<StoreCoverage> storeCoverageList)
        {
            // 1. Ưu tiên store có thể giao nhiều sản phẩm nhất
            var bestFulfillmentStore = storeCoverageList
                .OrderByDescending(x => x.FulfilledItemCount)
                .FirstOrDefault();

            if (bestFulfillmentStore != null)
            {
                var topStores = storeCoverageList
                    .Where(x => x.FulfilledItemCount == bestFulfillmentStore.FulfilledItemCount)
                    .ToList();

                // 2. Nếu có nhiều store có cùng số sản phẩm giao được, chọn store có Coverage Ratio cao nhất
                var bestCoverageStore = topStores
                    .OrderByDescending(x => x.CoverageRatio)
                    .FirstOrDefault();

                if (bestCoverageStore != null)
                {
                    var topCoverageStores = topStores
                        .Where(x => x.CoverageRatio == bestCoverageStore.CoverageRatio)
                        .ToList();

                    // 3. Nếu vẫn có nhiều store, chọn store có tổng hàng tồn kho nhiều nhất
                    var maxStockStore = topCoverageStores
                        .OrderByDescending(x => x.StockQuantity)
                        .FirstOrDefault();

                    if (maxStockStore != null)
                        return maxStockStore.StoreId;
                }
            }

            // 4. Nếu vẫn không có store nào nổi bật, chọn ngẫu nhiên
            return storeCoverageList.OrderBy(x => Guid.NewGuid()).First().StoreId;
        }

        private record StoreCoverage
        {
            public int StoreId { get; init; }
            public int FulfilledItemCount { get; init; }
            public double CoverageRatio { get; init; }
            public int StockQuantity { get; init; }
        }

        private record StoreFulfillmentData
        {
            public int FulfilledItemCount { get; init; }
            public double CoverageRatio { get; init; }
            public int TotalStock { get; init; }
        }
    }


}
