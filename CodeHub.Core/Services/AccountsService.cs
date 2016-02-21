using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CodeHub.Core.Data;
using SQLite;

namespace CodeHub.Core.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly SQLiteConnection _userDatabase;
        private readonly IDefaultValueService _defaults;
        private readonly string _accountsPath;

        public GitHubAccount ActiveAccount { get; private set; }

        public AccountsService(IDefaultValueService defaults, IAccountPreferencesService accountPreferences)
        {
            _defaults = defaults;
            _accountsPath = accountPreferences.AccountsDir;

            // Assure creation of the accounts path
            if (!Directory.Exists(_accountsPath))
                Directory.CreateDirectory(_accountsPath);

            _userDatabase = new SQLiteConnection(Path.Combine(_accountsPath, "accounts.db"));
            _userDatabase.CreateTable<GitHubAccount>();
        }

        public GitHubAccount GetDefault()
        {
            int id;
            return !_defaults.TryGet("DEFAULT_ACCOUNT", out id) ? null : Find(id);
        }

        public void SetDefault(GitHubAccount account)
        {
            if (account == null)
                _defaults.Clear("DEFAULT_ACCOUNT");
            else
                _defaults.Set("DEFAULT_ACCOUNT", account.Id);
        }

        public void SetActiveAccount(GitHubAccount account)
        {
            if (account != null)
            {
                var accountDir = CreateAccountDirectory(account);
                if (!Directory.Exists(accountDir))
                    Directory.CreateDirectory(accountDir);
            }

            ActiveAccount = account;
        }

        protected string CreateAccountDirectory(GitHubAccount account)
        {
            return Path.Combine(_accountsPath, account.Id.ToString(CultureInfo.InvariantCulture));
        }

        public void Insert(GitHubAccount account)
        {
            lock (_userDatabase)
            {
                _userDatabase.Insert(account);
            }
        }

        public void Remove(GitHubAccount account)
        {
            lock (_userDatabase)
            {
                _userDatabase.Delete(account);
            }
            var accountDir = CreateAccountDirectory(account);

            if (!Directory.Exists(accountDir))
                return;
            Directory.Delete(accountDir, true);
        }

        public void Update(GitHubAccount account)
        {
            lock (_userDatabase)
            {
                _userDatabase.Update(account);
            }
        }

        public bool Exists(GitHubAccount account)
        {
            return Find(account.Id) != null;
        }

        public GitHubAccount Find(int id)
        {
            lock (_userDatabase)
            {
                var query = _userDatabase.Find<GitHubAccount>(x => x.Id == id);
                return query;
            }
        }

        public IEnumerator<GitHubAccount> GetEnumerator()
        {
            foreach (var account in _userDatabase.Table<GitHubAccount>())
                yield return account;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
