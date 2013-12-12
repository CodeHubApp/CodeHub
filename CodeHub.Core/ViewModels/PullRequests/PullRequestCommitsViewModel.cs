using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommitsViewModel : ChangesetsViewModel
    {
		public ulong PullRequestId 
		{ 
			get; 
			private set; 
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
			public ulong PullRequestId { get; set; }
		}
    }
}

