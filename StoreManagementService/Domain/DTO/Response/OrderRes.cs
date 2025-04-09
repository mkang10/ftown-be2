using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int AccountId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Status { get; set; }
        public decimal? OrderTotal { get; set; }
        // Có thể thêm các trường khác nếu cần

        // Danh sách OrderAssignment được ánh xạ từ OrderAssignments
        public List<OrderAssignmentDTO> OrderAssignments { get; set; }

        public class OrderAssignmentDTO
        {
            public int AssignmentId { get; set; }
            public int OrderId { get; set; }
            public int ShopManagerId { get; set; }
            public int StaffId { get; set; }
            public DateTime AssignmentDate { get; set; }
            public string? Comments { get; set; }
        }

        // Dùng để trả về cho client

        public class OrderWithBuyerDTO
        {
            public int OrderId { get; set; }
            public DateTime? CreatedDate { get; set; }
            public string? Status { get; set; }
            public string BuyerName { get; set; } = null!;
            public List<AssignmentDTO> Assignments { get; set; } = new();


        }

        public class AssignmentDTO
        {
            public int ShopManagerId { get; set; }
            public int? StaffId { get; set; }
            public DateTime? AssignmentDate { get; set; }
            public string? Comments { get; set; }
        }
    }
}
