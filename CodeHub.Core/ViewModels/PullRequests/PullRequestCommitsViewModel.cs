using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Changesets;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommitsViewModel : ChangesetsViewModel
    {
        public long PullRequestId 
        { 
            get; 
            private set; 
        }

        public PullRequestCommitsViewModel(IApplicationService applicationService, IFeaturesService featuresService)
            : base(applicationService, featuresService)
        {
        }

        public void Init(NavObject navObject)
        {
            base.Init(navObject);
            PullRequestId = navObject.PullRequestId;
        }

        protected override GitHubRequest<List<CommitModel>> GetRequest()
        {
            return this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId].GetCommits();
        }

        public new class NavObject : CommitsViewModel.NavObject
        {
            public long PullRequestId { get; set; }
        }
    }
}

