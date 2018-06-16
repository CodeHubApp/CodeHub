using CodeHub.Core.Data;
using System;
using System.Threading.Tasks;
using CodeHub.Core.Utilities;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IAccountsService _accountsService;

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
            var domain = account.Domain ?? "https://api.github.com";
            Octokit.Credentials credentials = null;

            if (!string.IsNullOrEmpty(account.OAuth))
                credentials = new Octokit.Credentials(account.OAuth);
            else if (account.IsEnterprise || !string.IsNullOrEmpty(account.Password))
                credentials = new Octokit.Credentials(account.Username, account.Password);

            var octoClient = OctokitClientFactory.Create(new Uri(domain), credentials);
            var user = await octoClient.User.Current();
            account.Username = user.Login;
            account.AvatarUrl = user.AvatarUrl;

            await _accountsService.Save(account);
            await _accountsService.SetActiveAccount(account);

            Account = account;
            GitHubClient = octoClient;
        }
    }
}
