﻿using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly FtownContext _context;

        public UserManagementRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Role> CreateRole(Role role)
        {
            _context.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<ShopManagerDetail> CreateShopmanagerDetail(ShopManagerDetail user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Account> CreateUser(Account user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteRole(Role role)
        {
            _context.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUser(Account user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Role>> GetAllRole()
        {
            var data = await _context.Roles.ToListAsync();
            return data;
        }

      
        public async Task<Pagination<Account>> GetAllUser(PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Accounts.CountAsync();
            var items = await _context.Accounts
                                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Account>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<Role> GetRoleById(int id)
        {
            var data = await _context.Roles.SingleOrDefaultAsync(x => x.RoleId.Equals(id));
            return data;
        }

        public async Task<ShopManagerDetail> GetShopManagerdetailById(int id)
        {
            var data = await _context.ShopManagerDetails.SingleOrDefaultAsync(x => x.AccountId.Equals(id));
            return data;      }

        public async Task<Account> GetUserByGmail(string gmail)
        {
            var data = await _context.Accounts.SingleOrDefaultAsync(x => x.Email.Equals(gmail));
            return data;
        }

        public async Task<Account> GetUserById(int id)
        {
            var data = await _context.Accounts.SingleOrDefaultAsync(x => x.AccountId.Equals(id));
            return data;
        }

        public async Task<Account> GetUserByName(string name)
        {
            var data = await _context.Accounts.SingleOrDefaultAsync(x => x.FullName.Equals(name));
            return data;
        }

        public async Task<Role> UpdateRole(Role role)
        {
            _context.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<ShopManagerDetail> UpdateShopmanagerDetail(ShopManagerDetail user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Account> UpdateUser(Account user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
