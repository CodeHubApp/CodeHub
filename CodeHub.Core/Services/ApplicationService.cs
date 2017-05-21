using CodeHub.Core.Data;
using GitHubSharp;
using System;
using System.Threading.Tasks;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IAccountsService _accountsService;

        public Client Client { get; private set; }

        public Octokit.GitHubClient GitHubClient { get; private set; }

        public Account Account { get; private set; }

        public Action ActivationAction { get; set; }

        public ApplicationService(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        public void DeactivateUser()
        {
            _accountsService.SetActiveAccount(null).Wait();
            Account = null;
            Client = null;
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
                ActivationAction = action;
        }

        public Task UpdateActiveAccount() => _accountsService.Save(Account);

        public async Task LoginAccount(Account account)
        {
            var domain = account.Domain ?? Client.DefaultApi;
            Client client = null;
            Octokit.Credentials credentials = null;

            if (!string.IsNullOrEmpty(account.OAuth))
            {
                client = Client.BasicOAuth(account.OAuth, domain);
                credentials = new Octokit.Credentials(account.OAuth);
            }
            else if (account.IsEnterprise || !string.IsNullOrEmpty(account.Password))
            {
                client = Client.Basic(account.Username, account.Password, domain);
                credentials = new Octokit.Credentials(account.Username, account.Password);
            }

            var octoClient = OctokitClientFactory.Create(new Uri(domain), credentials);
            var user = await octoClient.User.Current();
            account.Username = user.Login;
            account.AvatarUrl = user.AvatarUrl;
            client.Username = user.Login;

            await _accountsService.Save(account);
            await _accountsService.SetActiveAccount(account);

            Account = account;
            Client = client;
            GitHubClient = octoClient;
        }
    }
}
