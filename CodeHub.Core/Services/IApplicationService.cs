using System;
using CodeHub.Core.Services;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IApplicationService
    {
        GitHubSharp.Client Client { get; }
 
        GitHubAccount Account { get; }

        IAccountsService Accounts { get; }

        void DeactivateUser();

        void ActivateUser(GitHubAccount account, GitHubSharp.Client client);

        void SetUserActivationAction(Action action);

        Action ActivationAction { get; set; }
    }
}