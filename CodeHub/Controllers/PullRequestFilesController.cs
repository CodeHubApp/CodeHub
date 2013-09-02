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
        private readonly string _username;
        private readonly string _slug;
        private readonly long _pullRequestId;

        public PullRequestFilesController(IListView<CommitModel.CommitFileModel> view, string username, string slug, long pullRequestId)
            : base(view)
        {
            _username = username;
            _slug = slug;
            _pullRequestId = pullRequestId;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].PullRequests[_pullRequestId].GetFiles(force);
            Model = new ListModel<CommitModel.CommitFileModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

