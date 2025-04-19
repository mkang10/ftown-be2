using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class UserRequestDTO
    {
        public int AccountId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }

        public int RoleId { get; set; }

        public string? ImagePath { get; set; }

        
    }
    public class CreateUserFullResponseDTO
    {
        public UserRequestDTO User { get; set; } = null!;
        public string? Token { get; set; }
    }


    public class CreateUserRequestWithPasswordDTO
    {
        
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }

        public int RoleId { get; set; }
        public IFormFile ImgFile { get; set; }
        public string? ImagePath { get; set; }
    }

    public class CreateShopmanagerDetailRequest    {
        public int AccountId { get; set; }

        public int StoreId { get; set; }

        public DateTime? ManagedDate { get; set; }

        public int? YearsOfExperience { get; set; }

        public string? ManagerCertifications { get; set; }

        public string? OfficeContact { get; set; }
    }


}
