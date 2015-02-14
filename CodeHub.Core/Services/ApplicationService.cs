using System.Reactive.Linq;
using CodeHub.Core.Data;
using GitHubSharp;
using System;
using ReactiveUI;
using Octokit;
using Octokit.Internal;
using System.Diagnostics;
using CodeHub.Core.Utilities;
using Splat;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IAccountsService _accountsService;
        private Action _activationAction;

        public Client Client { get; private set; }

        public IGitHubClient GitHubClient { get; private set; }

        public GitHubAccount Account
        {
            get { return _accountsService.ActiveAccount; }
        }

        public ApplicationService(IAccountsService accountsService)
        {
            _accountsService = accountsService;

            accountsService.WhenAnyObservable(x => x.ActiveAccountChanged)
                .StartWith(accountsService.ActiveAccount)
                .Subscribe(account =>
                {
                    if (account == null)
                    {
                        Client = null;
                        GitHubClient = null;
                    }
                    else
                    {
                        var githubAccount = account;
                        var domain = githubAccount.Domain ?? Client.DefaultApi;
                        Credentials credentials;

                        if (!string.IsNullOrEmpty(githubAccount.OAuth))
                        {
                            Client = Client.BasicOAuth(githubAccount.OAuth, domain);
                            credentials = new Credentials(githubAccount.OAuth);
                        }
                        else if (githubAccount.IsEnterprise || !string.IsNullOrEmpty(githubAccount.Password))
                        {
                            Client = Client.Basic(githubAccount.Username, githubAccount.Password, domain);
                            credentials = new Credentials(githubAccount.Username, githubAccount.Password);
                        }
                        else
                        {
                            Debugger.Break();
                            return;
                        }

                        var connection = new Connection(
                            new ProductHeaderValue("CodeHub"),
                            new Uri(domain),
                            new InMemoryCredentialStore(credentials),
                            new OctokitNetworkClient(new HttpClientAdapter(), Locator.Current.GetService<INetworkActivityService>()),
                            new Octokit.Internal.SimpleJsonSerializer());
                        GitHubClient = new GitHubClient(connection);
                    }
                });
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
                _activationAction = action;
        }

//        private async Task PromptForPushNotifications()
//        {
//            // Push notifications are not enabled for enterprise
//            if (Account.IsEnterprise)
//                return;
//
//            try
//            {
//                // Check for push notifications
//                if (Account.IsPushNotificationsEnabled == null && _features.IsPushNotificationsActivated)
//                {
//                    var result = await _alertDialogService.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
//                    if (result)
//                        Task.Run(() => _pushNotifications.Register()).FireAndForget();
//                    Account.IsPushNotificationsEnabled = result;
//                    Accounts.Update(Account);
//                }
//                else if (Account.IsPushNotificationsEnabled.HasValue && Account.IsPushNotificationsEnabled.Value)
//                {
//                    Task.Run(() => _pushNotifications.Register()).FireAndForget();
//                }
//            }
//            catch (Exception e)
//            {
//                _alertDialogService.Alert("Error", e.Message);
//            }
//        }
//
//        private static void CheckCacheSize(AccountCache cache)
//        {
//            var totalCacheSize = cache.Sum(x => System.IO.File.Exists(x.Path) ? new System.IO.FileInfo(x.Path).Length : 0);
//            var totalCacheSizeMB = (totalCacheSize / 1024f / 1024f);
//
//            if (totalCacheSizeMB > 64)
//            {
//                System.Console.WriteLine("Flushing cache due to size...");
//                cache.DeleteAll();
//            }
//        }
    }
}
