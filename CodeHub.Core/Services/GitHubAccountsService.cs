using CodeHub.Core.Data;
using System.Reactive.Subjects;
using System;
using System.Collections.Generic;
using Akavache;
using System.Reactive.Linq;
using System.Collections;
using System.Linq;
using Xamarin.Utilities.Services;

namespace CodeHub.Core.Services
{
    public class GitHubAccountsService : IAccountsService
    {
        private readonly Subject<GitHubAccount> _accountSubject = new Subject<GitHubAccount>();
        private readonly IDefaultValueService _defaults;
        private GitHubAccount _activeAccount;

        public GitHubAccount ActiveAccount
        {
            get { return _activeAccount; }
            set
            {
                _defaults.Set("DEFAULT_ACCOUNT", value == null ? null : value.Key);
                _activeAccount = value;
                _accountSubject.OnNext(value);
            }
        }

        public IObservable<GitHubAccount> ActiveAccountChanged
        {
            get { return _accountSubject; }
        }

        public GitHubAccountsService(IDefaultValueService defaults)
        {
            _defaults = defaults;
        }

        public GitHubAccount GetDefault()
        {
            string id;
            return !_defaults.TryGet("DEFAULT_ACCOUNT", out id) ? null : Find(id);
        }

        public void Insert(GitHubAccount account)
        {
            BlobCache.UserAccount.InsertObject("user_" + account.Key, account).Wait();
        }

        public void Remove(GitHubAccount account)
        {
            BlobCache.UserAccount.Invalidate("user_" + account.Key).Wait();
        }

        public void Update(GitHubAccount account)
        {
            Insert(account);
        }

        public bool Exists(GitHubAccount account)
        {
            return Find(account.Domain, account.Username) != null;
        }

        public GitHubAccount Find(string domain, string username)
        {
            return Find("user_" + domain + username);
        }

        public GitHubAccount Find(string key)
        {
            try
            {
                return BlobCache.UserAccount.GetObject<GitHubAccount>("user_" + key).Wait();
            }
            catch
            {
                return null;
            }
        }

        public IEnumerator<GitHubAccount> GetEnumerator()
        {
            var e = BlobCache.UserAccount.GetAllKeys().Wait()
                .Where(x => x.StartsWith("user_", StringComparison.Ordinal))
                .Select(k => BlobCache.UserAccount.GetObject<GitHubAccount>(k).Wait());
            return e.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}