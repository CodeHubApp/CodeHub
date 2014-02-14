using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using CodeFramework.Core.Data;
using CodeFramework.Core.Services;
using CodeHub.Core.Data;
using CodeHub.Core.ViewModels.App;
using GitHubSharp;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IMvxViewDispatcher _viewDispatcher;
        private readonly IPushNotificationsService _pushNotifications;
        private readonly IFeaturesService _features;
        private readonly IAlertDialogService _alertDialogService;
        private Action _activationAction;

        public Client Client { get; private set; }
        public GitHubAccount Account { get; private set; }
        public IAccountsService Accounts { get; private set; }


        public ApplicationService(IAccountsService accounts, IMvxViewDispatcher viewDispatcher, 
            IFeaturesService features, IPushNotificationsService pushNotifications,
            IAlertDialogService alertDialogService)
        {
            _viewDispatcher = viewDispatcher;
            _pushNotifications = pushNotifications;
            Accounts = accounts;
            _features = features;
            _alertDialogService = alertDialogService;
        }

        public void DeactivateUser()
        {
            Accounts.SetActiveAccount(null);
            Accounts.SetDefault(null);
            Account = null;
            Client = null;
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

            //Assign the cache
			Client.Cache = new GitHubCache(account);

            // Show the menu & show a page on the slideout
            _viewDispatcher.ShowViewModel(new MvxViewModelRequest {ViewModelType = typeof (MenuViewModel)});

            // A user has been activated!
            if (_activationAction != null)
            {
                _activationAction();
                _activationAction = null;
            }

            //Activate push notifications
            PromptForPushNotifications();
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
                _activationAction = action;
        }

        private async Task PromptForPushNotifications()
        {
            // Push notifications are not enabled for enterprise
            if (Account.IsEnterprise)
                return;

            try
            {
                // Check for push notifications
                if (Account.IsPushNotificationsEnabled == null && _features.IsPushNotificationsActivated)
                {
                    var result = await _alertDialogService.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
                    if (result)
                        Task.Run(() => _pushNotifications.Register()).FireAndForget();
                    Account.IsPushNotificationsEnabled = result;
                    Accounts.Update(Account);
                }
                else if (Account.IsPushNotificationsEnabled.HasValue && Account.IsPushNotificationsEnabled.Value)
                {
                    Task.Run(() => _pushNotifications.Register()).FireAndForget();
                }
            }
            catch (Exception e)
            {
                _alertDialogService.Alert("Error", e.Message);
            }
        }

		private static void CheckCacheSize(AccountCache cache)
		{
			var totalCacheSize = cache.Sum(x => System.IO.File.Exists(x.Path) ? new System.IO.FileInfo(x.Path).Length : 0);
			var totalCacheSizeMB = (totalCacheSize / 1024f / 1024f);

			if (totalCacheSizeMB > 64)
			{
				System.Console.WriteLine("Flushing cache due to size...");
				cache.DeleteAll();
			}
		}

		private class GitHubCache : ICache
		{
			private readonly AccountCache _account;
			public GitHubCache(Account account)
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

            public byte[] Get(string url)
			{
                var data = _account.Get(url);
				if (data == null)
                    return null;

				System.Console.WriteLine("[GET] cache: {0}", url);
				return data;
			}

            public void Set(string url, byte[] data, string etag)
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
