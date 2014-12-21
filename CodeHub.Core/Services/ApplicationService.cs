using System.Reactive.Linq;
using CodeHub.Core.Data;
using GitHubSharp;
using System;
using ReactiveUI;
using Octokit;
using Octokit.Internal;

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
            get { return _accountsService.ActiveAccount as GitHubAccount; }
        }

        public ApplicationService(IAccountsService accountsService)
        {
            _accountsService = accountsService;


            accountsService.WhenAnyObservable(x => x.ActiveAccountChanged).StartWith(accountsService.ActiveAccount).Subscribe(account =>
            {
                if (account == null)
                {
                    Client = null;
                    GitHubClient = null;
                }
                else
                {
                    var githubAccount = (GitHubAccount) account;
                    var domain = githubAccount.Domain ?? Client.DefaultApi;
                    if (!string.IsNullOrEmpty(githubAccount.OAuth))
                    {
                        Client = Client.BasicOAuth(githubAccount.OAuth, domain);
                        GitHubClient = new GitHubClient(new ProductHeaderValue("CodeHub"), 
                            new InMemoryCredentialStore(new Credentials(githubAccount.OAuth)), 
                            new Uri(domain));
                    }
                    else if (githubAccount.IsEnterprise || !string.IsNullOrEmpty(githubAccount.Password))
                    {
                        Client = Client.Basic(githubAccount.Username, githubAccount.Password, domain);
                        GitHubClient = new GitHubClient(new ProductHeaderValue("CodeHub"), 
                            new InMemoryCredentialStore(new Credentials(githubAccount.Username, githubAccount.Password)), 
                            new Uri(domain));
                    }
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

        private class GitHubCache : ICache
        {
            public T Get<T>(string url)
            {
                throw new NotImplementedException();
            }

            public void Set<T>(string url, T data)
            {
                throw new NotImplementedException();
            }
        }
    }
}
