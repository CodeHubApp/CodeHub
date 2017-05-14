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

        public void ActivateUser(Account account, Client client)
        {
            _accountsService.SetActiveAccount(account).Wait();
            Account = account;
            Client = client;

            var domain = account.Domain ?? Client.DefaultApi;
            var credentials = new Octokit.Credentials(account.OAuth);
            var oldClient = Client.BasicOAuth(account.OAuth, domain);
            GitHubClient = OctokitClientFactory.Create(new Uri(domain), credentials);
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
                ActivationAction = action;
        }

        public Task UpdateActiveAccount() => _accountsService.Save(Account);
    }
}
