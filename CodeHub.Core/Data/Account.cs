using System;
using System.Globalization;
using System.IO;
using CodeHub.Core.Services;
using MvvmCross.Platform;
using SQLite;

namespace CodeHub.Core.Data
{
    public abstract class Account : IDisposable
    {
        private SQLiteConnection _database;
        private AccountFilters _filters;
        private AccountPinnedRepositories _pinnedRepositories;

        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        /// <value>The avatar URL.</value>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the startup view when the account is loaded
        /// </summary>
        /// <value>The startup view.</value>
        public string DefaultStartupView { get; set; }

        [Ignore]
        public SQLiteConnection Database 
        {
            get
            {
                if (_database == null)
                {
                    if (!Directory.Exists(AccountDirectory))
                        Directory.CreateDirectory(AccountDirectory);

                    var dbPath = Path.Combine(AccountDirectory, "settings.db");
                    _database = new SQLiteConnection(dbPath);
                    return _database;
                }

                return _database;
            }
        }

        [Ignore]
        public string AccountDirectory
        {
            get
            {
                var accountsDir = Mvx.Resolve<IAccountPreferencesService>().AccountsDir;
                return Path.Combine(accountsDir, Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Ignore]
        public AccountFilters Filters
        {
            get
            {
                return _filters ?? (_filters = new AccountFilters(Database));
            }
        }

        [Ignore]
        public AccountPinnedRepositories PinnnedRepositories
        {
            get
            {
                return _pinnedRepositories ?? (_pinnedRepositories = new AccountPinnedRepositories(Database));
            }
        }

        private void CreateAccountDirectory()
        {
            if (!Directory.Exists(AccountDirectory))
                Directory.CreateDirectory(AccountDirectory);
        }

        /// <summary>
        /// This creates this account's directory
        /// </summary>
        public void Initialize()
        {
            CreateAccountDirectory();
        }

        /// <summary>
        /// This destorys this account's directory
        /// </summary>
        public void Destory()
        {
            if (!Directory.Exists(AccountDirectory))
                return;
            Directory.Delete(AccountDirectory, true);
        }
        

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Account"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Account"/>.</returns>
        public override string ToString()
        {
            return Username;
        }

        public void Dispose()
        {
            if (_database != null) _database.Dispose();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Account"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Account"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Account"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var act = obj as Account;
            return act != null && this.Id.Equals(act.Id);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}

