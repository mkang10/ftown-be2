using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class EditProfileRequest
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public IFormFile? AvatarImage { get; set; } // 👈 Thêm trường ảnh đại diện
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PreferredPaymentMethod { get; set; }
    }
}
