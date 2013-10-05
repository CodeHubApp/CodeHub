using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeFramework.Filters.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Controllers
{
    public class PullRequestCommitsController : ListController<CommitModel>
    {
        public string User { get; private set; }

        public string Repo { get; private set; }

        public long PullRequestId { get; private set; }

        public PullRequestCommitsController(IListView<CommitModel> view, string user, string repo, long pullRequestId)
            : base(view)
        {
            User = user;
            Repo = repo;
            PullRequestId = pullRequestId;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].GetCommits(), forceDataRefresh, response => {
                RenderView(new ListModel<CommitModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

