using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAccountRepos
    {
        Task<Account> GetUserByUsernameAsync(string email);
        Task AddUserAsync(Account user);

        Task AddStaffAsync(StaffDetail staff);

        Task AddShopmanagerAsync(ShopManagerDetail shopManager);

        Task AddCustomerAsync(CustomerDetail cus);

        Task<object?> GetRoleDetailsAsync(Account account);


    }


}
