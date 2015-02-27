using CodeHub.Core.Data;
using System;
using System.Collections.Generic;
using Akavache;
using System.Linq;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;

namespace CodeHub.Core.Data
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly IDefaultValueService _defaults;

        public AccountsRepository(IDefaultValueService defaults)
        {
            _defaults = defaults;
        }

        public Task SetDefault(GitHubAccount account)
        {
            _defaults.Set("DEFAULT_ACCOUNT", account == null ? null : account.Key);
            return Task.FromResult(0);
        }

        public Task<GitHubAccount> GetDefault()
        {
            string id;
            return !_defaults.TryGet("DEFAULT_ACCOUNT", out id) ? null : Find(id);
        }

        public Task Insert(GitHubAccount account)
        {
            return BlobCache.UserAccount.InsertObject("user_" + account.Key, account).ToTask();
        }

        public Task Remove(GitHubAccount account)
        {
            return BlobCache.UserAccount.Invalidate("user_" + account.Key).ToTask();
        }

        public Task Update(GitHubAccount account)
        {
            return Insert(account);
        }

        public async Task<bool> Exists(GitHubAccount account)
        {
            return (await Find(account.Domain, account.Username)) != null;
        }

        public Task<GitHubAccount> Find(string domain, string username)
        {
            return Find("user_" + domain + username);
        }

        public Task<GitHubAccount> Find(string key)
        {
            try
            {
                return BlobCache.UserAccount.GetObject<GitHubAccount>("user_" + key).ToTask();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<GitHubAccount>> GetAll()
        {
            var keys = await BlobCache.UserAccount.GetAllKeys().ToTask();
            var accounts = keys.Where(y => y.StartsWith("user_", StringComparison.Ordinal))
                .Select(k => BlobCache.UserAccount.GetObject<GitHubAccount>(k).ToTask());
            await Task.WhenAll(accounts);
            return accounts.Select(x => x.Result);
        }
    }
}