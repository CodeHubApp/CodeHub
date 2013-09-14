using CodeHub.Data;
using CodeFramework.Data;

namespace CodeHub
{
    /// <summary>
    /// Application.
    /// </summary>
    public static class Application
    {
        public static readonly Accounts<GitHubAccount> Accounts = new Accounts<GitHubAccount>();

        public static GitHubSharp.Client Client { get; private set; }

        public static GitHubAccount Account
        {
            get { return Accounts.ActiveAccount as GitHubAccount; }
            set { Accounts.SetActiveAccount(value); }
        }

        public static void UnsetUser()
        {
            Account = null;
            Client = null;
            Accounts.SetDefault(null);
        }

        public static void SetUser(GitHubAccount account, GitHubSharp.Client client)
        {
            if (account == null)
            {
                Account = null;
				Client = null;
                Accounts.SetDefault(null);
                return;
            }

            Accounts.SetActiveAccount(account);
            Accounts.SetDefault(account);

            //Assign the client
            Client = client;
            Client.Timeout = 1000 * 30;
            Client.CacheProvider = new AppCache();
        }

        /// <summary>
        /// A cache provider for GitHubSharp.
        /// Since the CodeFramework.Data.WebCacheProvider was modeled directly after the interface
        /// it can just inherit both and be alright. Otherwise, i'd have to do a little bit of work to make
        /// the proxy class.
        /// </summary>
        private class AppCache : CodeFramework.Cache.WebCacheProvider, GitHubSharp.ICacheProvider
        {
        }
    }
}

