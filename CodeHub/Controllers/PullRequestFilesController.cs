using System;
using GitHubSharp.Models;
using CodeFramework.Controllers;
using System.Collections.Generic;
using CodeHub.Filters.Models;
using System.Linq;

namespace CodeHub.Controllers
{
    public class PullRequestFilesController : ListController<CommitModel.CommitFileModel>
    {
        public long PullRequestId { get; private set; }
        public string Username { get; private set; }
        public string Repo { get; private set; }

        public PullRequestFilesController(IListView<CommitModel.CommitFileModel> view, string username, string slug, long pullRequestId)
            : base(view)
        {
            Username = username;
            Repo = slug;
            PullRequestId = pullRequestId;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[Username].Repositories[Repo].PullRequests[PullRequestId].GetFiles(force);
            Model = new ListModel<CommitModel.CommitFileModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

