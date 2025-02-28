using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class OrderHistoryResponse
    {
        public int OrderHistoryId { get; set; }
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public int ChangedBy { get; set; }
        public string ChangedByUser { get; set; } = null!;
        //ublic string ChangedByRole { get; set; } = null!;
        public DateTime ChangedDate { get; set; }
        public string? Comments { get; set; }
    }

}
