using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class LoginResponse
	{
		public string Token { get; set; }
		public AccountResponse Account { get; set; }
	}

    public class AccountResponse
    {
        public int AccountId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }

        public int RoleId { get; set; }

        public string? ImagePath { get; set; }
    }
}
