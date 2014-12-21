using CodeHub.Core.Services;
using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommitsViewModel : BaseCommitsViewModel
    {
        private readonly IApplicationService _applicationService;

        public long PullRequestId { get; set; }

        public PullRequestCommitsViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        protected override GitHubRequest<List<CommitModel>> CreateRequest()
        {
            return _applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].PullRequests[PullRequestId].GetCommits();
        }
    }
}

