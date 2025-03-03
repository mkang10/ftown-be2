using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class StoreStockResponse
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = null!;
        public int StockQuantity { get; set; }
    }
}
