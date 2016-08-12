using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IAccountsService
    {
        Task<Account> GetActiveAccount();

        Task SetActiveAccount(Account account);

        Task<IEnumerable<Account>> GetAccounts();

        Task Save(Account account);

        Task Remove(Account account);

        Task<Account> Get(string domain, string username);
    }
}