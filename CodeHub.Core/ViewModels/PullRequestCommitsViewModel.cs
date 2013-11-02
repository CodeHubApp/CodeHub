using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class PullRequestCommitsViewModel : ChangesetViewModel
    {
        public ulong PullRequestId 
        { 
            get; 
            private set; 
        }

        public PullRequestCommitsViewModel(string username, string repository, ulong pullRequestId)
            : base(username, repository)
        {
            PullRequestId = pullRequestId;
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<CommitModel>> GetRequest()
        {
            return Application.Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId].GetCommits();
        }
    }
}

