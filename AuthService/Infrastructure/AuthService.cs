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

        public async Task<LoginResponse> AuthenticateAsync(string username, string password)
        {
            var account = await _accountRepos.GetUserByUsernameAsync(username);

            if (account != null && VerifyPassword(password, account.PasswordHash))
            {
                string token = GenerateJwtToken(account.FullName, "user", account.AccountId, account.Email);

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

                return new LoginResponse
                {
                    Token = token,
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

            return null;
        }

        public async Task<TokenResponse> RegisterAsync(RegisterReq registerDTO)
        {
            var existingUser = await _accountRepos.GetUserByUsernameAsync(registerDTO.Username);

            if (existingUser != null)
                return new TokenResponse { Token = null };

            var account = new Account
            {
                FullName = registerDTO.Username,
                PasswordHash = HashPassword(registerDTO.Password),
                Email = registerDTO.Email,
                RoleId = registerDTO.RoleId
            };

            await _accountRepos.AddUserAsync(account);

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
    }
}
