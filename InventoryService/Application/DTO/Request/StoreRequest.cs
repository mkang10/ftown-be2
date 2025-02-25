using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class StoreRequest
    {
        public string StoreName { get; set; } = null!;
        public string? StoreDescription { get; set; }
        public string Location { get; set; } = null!;
        public int ManagerId { get; set; }
        public string? ImagePath { get; set; }
        public string? StoreEmail { get; set; }
        public string? StorePhone { get; set; }
        public string? OperatingHours { get; set; }
        // Không nhất thiết truyền CreatedDate từ FE, có thể set auto
    }
}
