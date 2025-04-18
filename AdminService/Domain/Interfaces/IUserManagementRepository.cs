using Domain.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserManagementRepository
    {
        public Task<Pagination<Account>> GetAllUser(PaginationParameter paginationParameter);

        public Task<Account> CreateUser(Account user);
        public Task<Account> UpdateUser(Account user);
        public Task<bool> DeleteUser(Account user);

        public Task<ShopManagerDetail> CreateShopmanagerDetail(ShopManagerDetail user);
        public Task<ShopManagerDetail> UpdateShopmanagerDetail(ShopManagerDetail user);
        public Task<ShopManagerDetail> GetShopManagerdetailById(int id);


        public Task<Account> GetUserById(int id);
        public Task<Account> GetUserByName(string name);
        public Task<Account> GetUserByGmail(string gmail);

        public Task<List<Role>> GetAllRole();
        public Task<Role> CreateRole(Role role);
        public Task<Role> UpdateRole(Role role);
        public Task<bool> DeleteRole(Role role);

        public Task<Role> GetRoleById(int id);



    }
}
