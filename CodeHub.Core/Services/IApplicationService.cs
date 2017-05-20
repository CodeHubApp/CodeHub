using System;
using System.Threading.Tasks;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IApplicationService
    {
        GitHubSharp.Client Client { get; }

        Octokit.GitHubClient GitHubClient { get; }
 
        Account Account { get; }

        Task UpdateActiveAccount();

        void DeactivateUser();

        void ActivateUser(Account account, GitHubSharp.Client client);

        void SetUserActivationAction(Action action);

        Action ActivationAction { get; set; }

        Task<Tuple<GitHubSharp.Client, Account>> LoginWithToken(string clientId, string clientSecret, string code, string redirect, string requestDomain, string apiDomain);

        Task<GitHubSharp.Client> LoginAccount(Account account);

        Task<Account> LoginWithBasic(string domain, string user, string pass, string twoFactor = null);
    }
}