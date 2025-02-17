using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAccountRepos
    {
        Task<Account> GetUserByUsernameAsync(string username);
        Task AddUserAsync(Account user);
    }
}
