using CodeHub.Data;
using CodeFramework.Data;

namespace CodeHub
{
    /// <summary>
    /// Application.
    /// </summary>
    public static class Application
    {
        public static GitHubSharp.Client Client { get; private set; }

        public static Accounts Accounts { get; private set; }

        public static Account Account
        {
            get { return Accounts.ActiveAccount; }
            set { Accounts.ActiveAccount = value; }
        }

        static Application()
        {
            Accounts = new Accounts(Database.Main);
        }

        public static void SetUser(CodeHub.Data.Account account)
        {
            if (account == null)
            {
                Account = null;
				Client = null;
                Accounts.SetDefault(null);
                return;
            }

            Accounts.ActiveAccount = account;
            Accounts.SetDefault(account);

            Client = new GitHubSharp.Client(account.Username, account.Password) { 
				Timeout = 1000 * 30, //30 seconds
                CacheProvider = new AppCache(),
			};
        }

        /// <summary>
        /// A cache provider for BitBucketSharp.
        /// Since the CodeFramework.Data.WebCacheProvider was modeled directly after the interface
        /// it can just inherit both and be alright. Otherwise, i'd have to do a little bit of work to make
        /// the proxy class.
        /// </summary>
        private class AppCache : CodeFramework.Data.WebCacheProvider, GitHubSharp.ICacheProvider
        {
        }
    }
}

