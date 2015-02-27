using System.Collections.Generic;
using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Data
{
    public interface IAccountsRepository
    {
        /// <summary>
        /// Sets the active account
        /// </summary>
        Task SetDefault(GitHubAccount account);

        /// <summary>
        /// Gets the default account
        /// </summary>
        Task<GitHubAccount> GetDefault();

        /// <summary>
        /// Gets all the accounts
        /// </summary>
        Task<IEnumerable<GitHubAccount>> GetAll();

        /// <summary>
        /// Insert the specified account.
        /// </summary>
        Task Insert(GitHubAccount account);

        /// <summary>
        /// Remove the specified account.
        /// </summary>
        Task Remove(GitHubAccount account);

        /// <summary>
        /// Update this instance in the database
        /// </summary>
        Task Update(GitHubAccount account);

        /// <summary>
        /// Checks to see whether a specific account exists (Username comparison)
        /// </summary>
        Task<bool> Exists(GitHubAccount account);

        /// <summary>
        /// Find the specified account via it's username
        /// </summary>
        Task<GitHubAccount> Find(string domain, string username);
    }
}