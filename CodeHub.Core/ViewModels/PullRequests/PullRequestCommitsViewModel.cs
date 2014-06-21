using CodeHub.Core.Services;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommitsViewModel : ChangesetsViewModel
    {
        public long PullRequestId { get; set; }

        public PullRequestCommitsViewModel(IApplicationService applicationService)
            : base(applicationService)
        {
        }

        protected override GitHubRequest<List<CommitModel>> GetRequest()
        {
            return ApplicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests[PullRequestId].GetCommits();
        }
    }
}

