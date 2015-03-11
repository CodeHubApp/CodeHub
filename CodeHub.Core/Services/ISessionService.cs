using CodeHub.Core.Data;
using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface ISessionService
    {
        GitHubAccount Account { get; }

        GitHubSharp.Client Client { get; }

        Octokit.IGitHubClient GitHubClient { get; }

        Task SetSessionAccount(GitHubAccount account);

        void SetStartupCommand(IStartupCommand startupCommand);

        Task RegisterForNotifications();
    }
}