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

        public static CodeFramework.Cache.CacheProvider ClientCache { get; private set; }

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

            //Set the cache
            ClientCache = account.Cache;
            Client.Cache = new GitHubCache(account.Cache);
        }
    }

    public class GitHubCache : GitHubSharp.ICache
    {
        private CodeFramework.Cache.CacheProvider _provider;

        public GitHubCache(CodeFramework.Cache.CacheProvider provider)
        {
            _provider = provider;
        }

        public string GetETag(string url)
        {
            var data = _provider.GetEntry(url);
            if (data == null)
                return null;
            return data.CacheTag;
        }

        public T Get<T>(string url) where T : new()
        {
            var data = _provider.Get<T>(url);
            if (data == null)
                return default(T);

            System.Console.WriteLine("[GET] cache: {0}", url);
            return data;
        }

        public void Set(string url, object data, string etag)
        {
            System.Console.WriteLine("[SET] cache: {0}", url);
            _provider.Set(url, data, etag);
        }

        public bool Exists(string url)
        {
            return _provider.GetEntry(url) != null;
        }
    }
}

