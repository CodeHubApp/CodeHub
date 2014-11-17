using System;
using CodeHub.Core.Data;

namespace CodeHub.Core.Services
{
    public interface IApplicationService
    {
        GitHubSharp.Client Client { get; }

        Octokit.IGitHubClient GitHubClient { get; }
 
        GitHubAccount Account { get; }

        void SetUserActivationAction(Action action);
    }
}