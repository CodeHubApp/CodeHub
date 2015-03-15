using CodeHub.Core.Data;
using GitHubSharp;
using System;
using Octokit;
using System.Diagnostics;
using CodeHub.Core.Utilities;
using System.Threading.Tasks;
using System.Net.Http;
using ModernHttpClient;

namespace CodeHub.Core.Services
{
    public class SessionService : ISessionService
    {
        private readonly IAccountsRepository _accountsRepository;

        static SessionService()
        {
            GitHubSharp.Client.ClientConstructor = 
                () => new HttpClient(new NativeMessageHandler());
        }

        public Client Client { get; private set; }

        public IGitHubClient GitHubClient { get; private set; }

        public GitHubAccount Account { get; private set; }

        public SessionService(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }

        public async Task SetSessionAccount(GitHubAccount account)
        {
            if (account == null)
            {
                Account = null;
                Client = null;
                GitHubClient = null;
                return;
            }

            try
            {
                var githubAccount = account;
                var domain = githubAccount.Domain ?? Client.DefaultApi;
                Credentials credentials;
                Client oldClient;

                if (!string.IsNullOrEmpty(githubAccount.OAuth))
                {
                    oldClient = Client.BasicOAuth(githubAccount.OAuth, domain);
                    credentials = new Credentials(githubAccount.OAuth);
                }
                else if (githubAccount.IsEnterprise || !string.IsNullOrEmpty(githubAccount.Password))
                {
                    oldClient = Client.Basic(githubAccount.Username, githubAccount.Password, domain);
                    credentials = new Credentials(githubAccount.Username, githubAccount.Password);
                }
                else
                {
                    Debugger.Break();
                    return;
                }
            
                var newClient = OctokitClientFactory.Create(new Uri(domain), credentials);
                var userInfo = await newClient.User.Current();
                account.Name = userInfo.Name;
                account.Email = userInfo.Email;
                account.AvatarUrl = userInfo.AvatarUrl;
                await _accountsRepository.Update(account);

                // Set all the good stuff.
                Client = oldClient;
                GitHubClient = newClient;
                Account = account;
            }
            catch
            {
                Account = null;
                GitHubClient = null;
                Client = null;
                throw;
            }
        }

        public Task RegisterForNotifications()
        {
//            var accounts = Locator.Current.GetService<IAccountsRepository>();
//            if (app.Account != null && !app.Account.IsPushNotificationsEnabled.HasValue)
//            {
//                Task.Run(() => Locator.Current.GetService<IPushNotificationsService>().Register());
//                app.Account.IsPushNotificationsEnabled = true;
//                accounts.Update(app.Account);
//            }
            return Task.FromResult(0);
        }

        public void SetStartupCommand(IStartupCommand startupCommand)
        {
//            try
//            {
//                var serviceConstructor = Locator.Current.GetService<IServiceConstructor>();
//                var appService = Locator.Current.GetService<ISessionService>();
//                var accounts = Locator.Current.GetService<IAccountsRepository>();
//                var username = data["u"].ToString();
//                var repoId = new RepositoryIdentifier(data["r"].ToString());
//
//                if (data.ContainsKey(new NSString("c")))
//                {
//                    var vm = serviceConstructor.Construct<CommitViewModel>();
//                    vm.RepositoryOwner = repoId.Owner;
//                    vm.RepositoryName = repoId.Name;
//                    vm.Node = data["c"].ToString();
//                    vm.ShowRepository = true;
//                }
//                else if (data.ContainsKey(new NSString("i")))
//                {
//                    var vm = serviceConstructor.Construct<CodeHub.Core.ViewModels.Issues.IssueViewModel>();
//                    vm.RepositoryOwner = repoId.Owner;
//                    vm.RepositoryName = repoId.Name;
//                    vm.Id = int.Parse(data["i"].ToString());
//                }
//                else if (data.ContainsKey(new NSString("p")))
//                {
//                    var vm = serviceConstructor.Construct<CodeHub.Core.ViewModels.PullRequests.PullRequestViewModel>();
//                    vm.RepositoryOwner = repoId.Owner;
//                    vm.RepositoryName = repoId.Name;
//                    vm.Id = int.Parse(data["p"].ToString());
//                }
//                else
//                {
//                    var vm = serviceConstructor.Construct<CodeHub.Core.ViewModels.Repositories.RepositoryViewModel>();
//                    vm.RepositoryOwner = repoId.Owner;
//                    vm.RepositoryName = repoId.Name;
//                }
//
//                if (appService.Account == null || !appService.Account.Username.Equals(username))
//                {
//                    var user = accounts.FirstOrDefault(x => x.Username.Equals(username));
//                    if (user != null)
//                    {
//                        accounts.ActiveAccount = user;
//                    }
//                }
//
//                //appService.SetUserActivationAction(() => transitionOrchestration.Transition);
//
//                if (appService.Account == null && !fromBootup)
//                {
//                    //                    var startupViewModelRequest = MvxViewModelRequest<CodeHub.Core.ViewModels.App.StartupViewModel>.GetDefaultRequest();
//                    //                    viewDispatcher.ShowViewModel(startupViewModelRequest);
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("Handle Notifications issue: " + e);
//            }
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
    }
}
