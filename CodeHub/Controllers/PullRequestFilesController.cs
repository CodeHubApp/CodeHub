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

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[Username].Repositories[Repo].PullRequests[PullRequestId].GetFiles(), forceDataRefresh, response => {
                RenderView(new ListModel<CommitModel.CommitFileModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

