using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommitsViewModel : BaseCommitsViewModel
    {
        public int PullRequestId { get; private set; }

        public PullRequestCommitsViewModel(ISessionService sessionService)
            : base(sessionService)
        {
        }

        protected override System.Uri RequestUri
        {
            get { return Octokit.ApiUrls.PullRequestCommits(RepositoryOwner, RepositoryName, PullRequestId); }
        }

        public PullRequestCommitsViewModel Init(string repositoryOwner, string repositoryName, int pullRequestId)
        {
            Init(repositoryOwner, repositoryName);
            PullRequestId = pullRequestId;
            return this;
        }
    }
}

