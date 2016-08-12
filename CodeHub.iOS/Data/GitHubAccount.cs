using System;
using System.Globalization;
using System.IO;
using SQLite;

namespace CodeHub.iOS.Data
{
    public class GitHubAccount : IDisposable
    {
        /// <summary>
        /// Gets or sets the OAuth string
        /// </summary>
        public string OAuth { get; set; }

        /// <summary>
        /// The password which is only used on Enterprise accounts since oAuth does not work.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the domain (API)
        /// </summary>
        /// <value>The domain.</value>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the web domain. Sort of like the API domain with the API paths
        /// </summary>
        /// <value>The web domain.</value>
        public string WebDomain { get; set; }

        /// <summary>
        /// Gets whether this account is enterprise or not
        /// </summary>
        /// <value><c>true</c> if enterprise; otherwise, <c>false</c>.</value>
        public bool IsEnterprise { get; set; }

        /// <summary>
        /// Gets or sets whether orgs should be listed in the menu controller under 'events'
        /// </summary>
        public bool ShowOrganizationsInEvents { get; set; }

        /// <summary>
        /// Gets or sets whether teams & groups should be expanded in the menu controller to their actual contents
        /// </summary>
        public bool ExpandOrganizations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Account"/> hides the repository
        /// description in list.
        /// </summary>
        /// <value><c>true</c> if hide repository description in list; otherwise, <c>false</c>.</value>
        public bool ShowRepositoryDescriptionInList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CodeHub.Core.Data.GitHubAccount"/> push notifications enabled.
        /// </summary>
        /// <value><c>true</c> if push notifications enabled; otherwise, <c>false</c>.</value>
        public bool? IsPushNotificationsEnabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Account"/> class.
        /// </summary>
        public GitHubAccount()
        {
            //Set some default values
            ShowOrganizationsInEvents = true;
            ExpandOrganizations = true;
            ShowRepositoryDescriptionInList = true;
        }

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
                var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..");
                var accountsDir = Path.Combine(baseDir, "Documents/accounts");
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

        public void Dispose()
        {
            if (_database != null) _database.Dispose();
        }
    }
}

