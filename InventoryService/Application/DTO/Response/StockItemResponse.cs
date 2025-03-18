using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class StockItemResponse
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public int WarehouseId { get; set; }

        public StockItemResponse(int variantId, int quantity, int warehouseId)
        {
            VariantId = variantId;
            Quantity = quantity;
            WarehouseId = warehouseId;
        }
    }
}
