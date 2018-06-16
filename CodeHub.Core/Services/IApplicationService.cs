using System;
using System.Threading.Tasks;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IApplicationService
    {
        Octokit.GitHubClient GitHubClient { get; }
 
        Account Account { get; }

        Task UpdateActiveAccount();

        void DeactivateUser();

        void SetUserActivationAction(Action action);

        Action ActivationAction { get; set; }

        Task LoginAccount(Account account);
    }
}