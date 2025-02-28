using Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class StockUpdateRequest
    {
        public int StoreId { get; set; }
        public List<StockItemResponse> Items { get; set; } = new();
    }
}
