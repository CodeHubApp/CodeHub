using System;
using System.Collections.Generic;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IAccountsService : IEnumerable<GitHubAccount>
    {
        /// <summary>
        /// Gets the active account
        /// </summary>
        GitHubAccount ActiveAccount { get; set; }

        /// <summary>
        /// Gets the default account
        /// </summary>
        GitHubAccount GetDefault();

        /// <summary>
        /// Insert the specified account.
        /// </summary>
        void Insert(GitHubAccount account);

        /// <summary>
        /// Remove the specified account.
        /// </summary>
        void Remove(GitHubAccount account);

        /// <summary>
        /// Update this instance in the database
        /// </summary>
        void Update(GitHubAccount account);

        /// <summary>
        /// Checks to see whether a specific account exists (Username comparison)
        /// </summary>
        bool Exists(GitHubAccount account);

        /// <summary>
        /// Find the specified account via it's username
        /// </summary>
        GitHubAccount Find(string domain, string username);

        /// <summary>
        /// An observable sequence of account changing events
        /// </summary>
        IObservable<GitHubAccount> ActiveAccountChanged { get; }
    }
}