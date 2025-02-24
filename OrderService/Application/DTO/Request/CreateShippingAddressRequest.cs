using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class CreateShippingAddressRequest
    {
        public int AccountId { get; set; }

        // Thông tin địa chỉ chi tiết
        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        [Required]
        public string Province { get; set; } = null!; // Tỉnh

        [Required]
        public string District { get; set; } = null!; // Huyện

        [Required]
        public string Country { get; set; } = null!;

        public string? PostalCode { get; set; }

        // Thông tin người nhận
        [Required]
        public string RecipientName { get; set; } = null!;

        [Required]
        public string RecipientPhone { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;

        // Có thể có IsDefault nếu muốn đánh dấu địa chỉ mặc định
        public bool? IsDefault { get; set; }
    }

}
