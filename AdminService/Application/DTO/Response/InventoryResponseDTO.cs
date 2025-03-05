using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class InventoryPendingResponseDto
    {
        public int ImportId { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public string? Status { get; set; }
        public string? ReferenceNumber { get; set; }
        public decimal? TotalCost { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        // Các thuộc tính khác nếu cần...
    }
}
