using System.Threading.Tasks;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : LoadableViewModel
    {
        public ReactiveList<Octokit.PullRequestFile> Files { get; } = new ReactiveList<Octokit.PullRequestFile>();

        public int PullRequestId { get; }

        public string Username { get; }

        public string Repository { get; }

        public string Sha { get; }

        public PullRequestFilesViewModel(
            string username,
            string repository,
            string sha,
            int pullRequestId)
        {
            Username = username;
            Repository = repository;
            Sha = sha;
            PullRequestId = pullRequestId;
        }

        protected override async Task Load()
        {
            var result = await this.GetApplication().GitHubClient.PullRequest.Files(Username, Repository, PullRequestId);
            Files.Reset(result);
        }
    }
}

