using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class StoreResponse
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = null!;
        public string? StoreDescription { get; set; }
        public string Location { get; set; } = null!;
        public string? ImagePath { get; set; }
        public string? StoreEmail { get; set; }
        public string? StorePhone { get; set; }
    }

}
