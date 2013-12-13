using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using CodeFramework.Core;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;
using CodeHub.Core.ViewModels;
using CodeHub.Core.ViewModels.App;
using GitHubSharp;
using System.Linq;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IMvxViewDispatcher _viewDispatcher;
        public Client Client { get; private set; }
        public GitHubAccount Account { get; private set; }
        public IAccountsService Accounts { get; private set; }

        public ApplicationService(IAccountsService accounts, IMvxViewDispatcher viewDispatcher)
        {
            _viewDispatcher = viewDispatcher;
            Accounts = accounts;
        }

        public void ActivateUser(GitHubAccount account, Client client)
        {
            Accounts.SetActiveAccount(account);
            Account = account;
            Client = client;

			//Set the default account
			Accounts.SetDefault(account);

			//Check the cache size
			CheckCacheSize(account.Cache);

			Client.Cache = new GitHubCache(account);

            // Show the menu & show a page on the slideout
            _viewDispatcher.ShowViewModel(new MvxViewModelRequest {ViewModelType = typeof (MenuViewModel)});
        }

		private static void CheckCacheSize(CodeFramework.Core.Data.AccountCache cache)
		{
			var totalCacheSize = cache.Sum(x => System.IO.File.Exists(x.Path) ? new System.IO.FileInfo(x.Path).Length : 0);
			var totalCacheSizeMB = ((float)totalCacheSize / 1024f / 1024f);

			if (totalCacheSizeMB > 64)
			{
				System.Console.WriteLine("Flushing cache due to size...");
				cache.DeleteAll();
			}
		}

		private class GitHubCache : ICache
		{
			private readonly CodeFramework.Core.Data.AccountCache _account;
			public GitHubCache(GitHubAccount account)
			{
				_account = account.Cache;
			}

			public string GetETag(string url)
			{
				var data = _account.GetEntry(url);
				if (data == null)
					return null;
				return data.CacheTag;
			}

			public T Get<T>(string url) where T : new()
			{
				var data = _account.Get<T>(url);
				if (data == null)
					return default(T);

				System.Console.WriteLine("[GET] cache: {0}", url);
				return data;
			}

			public void Set(string url, object data, string etag)
			{
				System.Console.WriteLine("[SET] cache: {0}", url);
				_account.Set(url, data, etag);
			}

			public bool Exists(string url)
			{
				return _account.GetEntry(url) != null;
			}
		}
    }
}
