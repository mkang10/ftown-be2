using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class UpdateStoreDetailDto
    {
        // Giả sử ImportStoreDetail có một khóa chính là Id, nếu không bạn có thể sử dụng các thuộc tính tạo thành composite key
        public int ImportStoreDetailId { get; set; }
        public int ActualReceivedQuantity { get; set; }
        public string? Comment { get; set; }
    }
}
