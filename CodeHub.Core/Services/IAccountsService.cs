using System.Collections.Generic;
using CodeFramework.Core.Data;

namespace CodeFramework.Core.Services
{
    public interface IAccountsService : IEnumerable<IAccount>
    {
        /// <summary>
        /// Gets the active account
        /// </summary>
        IAccount ActiveAccount { get; }

        /// <summary>
        /// Sets the active account
        /// </summary>
        /// <param name="account"></param>
        void SetActiveAccount(IAccount account);

        /// <summary>
        /// Gets the default account
        /// </summary>
        IAccount GetDefault();

        /// <summary>
        /// Sets the default account
        /// </summary>
        void SetDefault(IAccount account);

        /// <summary>
        /// Insert the specified account.
        /// </summary>
        void Insert(IAccount account);

        /// <summary>
        /// Remove the specified account.
        /// </summary>
        void Remove(IAccount account);

        /// <summary>
        /// Update this instance in the database
        /// </summary>
        void Update(IAccount account);

        /// <summary>
        /// Checks to see whether a specific account exists (Username comparison)
        /// </summary>
        bool Exists(IAccount account);

        /// <summary>
        /// Find the specified account via it's username
        /// </summary>
        IAccount Find(int id);
    }
}