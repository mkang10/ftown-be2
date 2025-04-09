using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class WarehouseStockResponse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = null!;
        public int StockQuantity { get; set; }
    }
}
