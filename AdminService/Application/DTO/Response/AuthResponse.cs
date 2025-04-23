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
    public class AdminAccountSetting
    {
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AccountResponse
    {
        public int AccountId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool? IsActive { get; set; }

        public int RoleId { get; set; }

        public string? ImagePath { get; set; }

      
        public object? RoleDetails { get; set; } // Chứa thông tin chi tiết theo từng role
       
    }
    public class ForgotPasswordRequestCapcha
    {
        public string Email { get; set; }
        public string RecaptchaToken { get; set; }
    }

}
