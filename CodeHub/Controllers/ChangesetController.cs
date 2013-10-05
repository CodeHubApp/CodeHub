using GitHubSharp.Models;
using System.Collections.Generic;
using CodeFramework.Controllers;
using GitHubSharp;


namespace CodeHub.Controllers
{
    public class ChangesetController : ListController<CommitModel>
    {
        public string User { get; private set; }

        public string Slug { get; private set; }

        public ChangesetController(IListView<CommitModel> view, string user, string slug)
            : base(view)
        {
            User = user;
            Slug = slug;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(CreateRequest(), forceDataRefresh, response => {
                RenderView(new ListModel<CommitModel>(response.Data, this.CreateMore(response)));
            });
        }

        protected GitHubRequest<List<CommitModel>> CreateRequest(string startNode = null)
        {
            return Application.Client.Users[User].Repositories[Slug].Commits.GetAll(startNode);
        }
    }
}

