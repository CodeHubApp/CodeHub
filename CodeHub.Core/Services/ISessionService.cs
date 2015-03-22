using CodeHub.Core.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public interface ISessionService
    {
        GitHubAccount Account { get; }

        GitHubSharp.Client Client { get; }

        Octokit.IGitHubClient GitHubClient { get; }

        void Track(string eventName, IDictionary<string, string> properties = null);

        Task SetSessionAccount(GitHubAccount account);

        void SetStartupCommand(IStartupCommand startupCommand);

        Task RegisterForNotifications();
    }
}