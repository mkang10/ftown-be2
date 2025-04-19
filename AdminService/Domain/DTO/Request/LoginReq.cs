using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Request
{
    public class LoginReq
    {
        public string email { get; set; }
        public string Password { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = null!;
    }

}
