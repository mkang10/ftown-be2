﻿using Domain.DTO.Response;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AuthAdminHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IUserManagementRepository _userManagementRepository;
        private readonly AdminAccountSetting _adminAccount;

        public AuthAdminHandler(IConfiguration configuration, 
            IUserManagementRepository userManagementRepository,
            IOptions<AdminAccountSetting> adminAccount)
        {
            _configuration = configuration;
            _userManagementRepository = userManagementRepository;
            _adminAccount = adminAccount.Value;
        }

        public async Task<LoginResponse> AuthenticateAsync(string email, string password)
        {
            var data = await _userManagementRepository.GetUserByGmail(email);

            if (email == _adminAccount.Account && password == _adminAccount.Password)
            {
                string token = GenerateJwtToken(data.Email, data.Role.RoleName, data.AccountId, data.Email);

                if (data.IsActive == false)
                {
                    return new LoginResponse
                    {
                        Token = null,
                        Account = new AccountResponse
                        {
                            AccountId = data.AccountId,
                            FullName = data.FullName,
                            RoleId = data.RoleId,
                            IsActive = data.IsActive,
                            Email = data.Email
                        }
                    };
                }
                object? roleDetails = await _userManagementRepository.GetRoleDetailsAsync(data);

                return new LoginResponse
                {
                    Token = token,
                    Account = new AccountResponse
                    {
                        AccountId = data.AccountId,
                        FullName = data.FullName,
                        RoleId = data.RoleId,
                        IsActive = data.IsActive,
                        Email = data.Email,
                        RoleDetails = roleDetails 
                    }
                };
            }
            return null;
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
