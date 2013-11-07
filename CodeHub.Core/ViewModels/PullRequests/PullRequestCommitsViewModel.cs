using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommitsViewModel : ChangesetViewModel
    {
        public ulong PullRequestId 
        { 
            get; 
            private set; 
        }

        public void Init(NavObject navObject)
        {
#warning More work here...
            PullRequestId = navObject.PullRequestId;
        }

        protected override GitHubSharp.GitHubRequest<System.Collections.Generic.List<CommitModel>> GetRequest()
        {
            return Application.Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId].GetCommits();
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public ulong PullRequestId { get; set; }
        }
    }
}

