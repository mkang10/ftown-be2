using Application.Interfaces;
using Domain.Entities;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Domain.DTO.Response;
using Domain.DTO.Request;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepos _accountRepos;
        private readonly IConfiguration _configuration;

        public AuthService(IAccountRepos accountRepos, IConfiguration configuration)
        {
            _accountRepos = accountRepos;
            _configuration = configuration;
        }

        public async Task<LoginResponse> AuthenticateAsync(string email, string password)
        {
            var account = await _accountRepos.GetUserByUsernameAsync(email);

            if (account != null && VerifyPassword(password, account.PasswordHash))
            {
                string token = GenerateJwtToken(account.FullName, account.Role.RoleName, account.AccountId, account.Email);

                if (account.IsActive == false)
                {
                    return new LoginResponse
                    {
                        Token = null,
                        Account = new AccountResponse
                        {
                            AccountId = account.AccountId,
                            FullName = account.FullName,
                            RoleId = account.RoleId,
                            IsActive = account.IsActive,
                            Email = account.Email
                        }
                    };
                }

                // Lấy thông tin chi tiết dựa theo RoleId
                object? roleDetails = await _accountRepos.GetRoleDetailsAsync(account);

                return new LoginResponse
                {
                    Token = token,
                    Account = new AccountResponse
                    {
                        AccountId = account.AccountId,
                        FullName = account.FullName,
                        RoleId = account.RoleId,
                        IsActive = account.IsActive,
                        Email = account.Email,
                        RoleDetails = roleDetails // Chứa thông tin chi tiết theo vai trò
                    }
                };
            }

            return null;
        }

       

        public async Task<TokenResponse> RegisterAsync(RegisterReq registerDTO)
        {
            var existingUser = await _accountRepos.GetUserByUsernameAsync(registerDTO.Username);

            if (existingUser != null)
                return new TokenResponse { Token = null };

            // Tạo tài khoản mới
            var account = new Account
            {
                FullName = registerDTO.Username,
                PasswordHash = HashPassword(registerDTO.Password),
                Email = registerDTO.Email,
                RoleId = 1,
                // Các trường khác nếu có
            };

            // Thêm tài khoản vào database
            await _accountRepos.AddUserAsync(account);

           
            var customerDetail = new CustomerDetail
            {
                AccountId = account.AccountId,
                        LoyaltyPoints = 0,                       // Mặc định là 0 điểm
                        MembershipLevel = "Basic",               // Mức thành viên mặc định
                        DateOfBirth = null,
                        Gender = null,
                        CustomerType = null,
                        PreferredPaymentMethod = null
                    };
                    await _accountRepos.AddCustomerAsync(customerDetail);
                    

            // Tạo JWT token cho tài khoản mới
            string token = GenerateJwtToken(account.FullName, "user", account.AccountId, account.Email);

            return new TokenResponse { Token = token };
        }

        private string GenerateJwtToken(string username, string roleName, int userId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] computedHash = pbkdf2.GetBytes(20);

            return computedHash.SequenceEqual(hashBytes.Skip(16).Take(20));
        }

      

        public async Task<LoginResponse> AuthenticateWithGoogleAsync(string idToken)
        {
            // 1. Xác thực idToken với Google
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                // Token không hợp lệ
                return null;
            }

            // 2. Lấy email từ payload
            string email = payload.Email;
            string fullName = payload.Name;

            // 3. Kiểm tra account đã tồn tại chưa
            var account = await _accountRepos.GetUserByUsernameAsync(email);
            if (account == null)
            {
                // Tạo tài khoản mới
                account = new Account
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = null,    // không dùng password truyền thống
                    RoleId = 1,       // default user
                    IsActive = true
                };
                await _accountRepos.AddUserAsync(account);

                // Tạo chi tiết customer
                var customerDetail = new CustomerDetail
                {
                    AccountId = account.AccountId,
                    LoyaltyPoints = 0,
                    MembershipLevel = "Basic",
                    DateOfBirth = null,
                    Gender = null,
                    CustomerType = null,
                    PreferredPaymentMethod = null
                };
                await _accountRepos.AddCustomerAsync(customerDetail);
            }

            // 4. Nếu account bị khoá hoặc inactive
            if (!(bool)account.IsActive)
            {
                return new LoginResponse
                {
                    Token = null,
                    Account = new AccountResponse
                    {
                        AccountId = account.AccountId,
                        FullName = account.FullName,
                        RoleId = account.RoleId,
                        IsActive = account.IsActive,
                        Email = account.Email
                    }
                };
            }

            // 5. Sinh JWT và trả về
            string token = GenerateJwtToken(
                account.FullName,
                "user",
                account.AccountId,
                account.Email
            );

            object roleDetails = await _accountRepos.GetRoleDetailsAsync(account);

            return new LoginResponse
            {
                Token = token,
                Account = new AccountResponse
                {
                    AccountId = account.AccountId,
                    FullName = account.FullName,
                    RoleId = account.RoleId,
                    IsActive = account.IsActive,
                    Email = account.Email,
                    RoleDetails = roleDetails
                }
            };
        }


    }
}
