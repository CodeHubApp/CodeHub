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

        public override void Update(bool force)
        {
            var response = Application.Client.Users[User].Repositories[Repo].PullRequests[PullRequestId].GetCommits(force);
            Model = new ListModel<CommitModel> { Data = response.Data, More = this.CreateMore(response) };
        }
    }
}

