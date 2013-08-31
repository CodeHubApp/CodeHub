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

        public override void Update(bool force)
        {
            var response = GetData(force);
            Model = new ListModel<CommitModel> {Data = response.Data, More = this.CreateMore(response)};
        }

        protected GitHubResponse<List<CommitModel>> GetData(bool force, string startNode = null)
        {
            return Application.Client.Users[User].Repositories[Slug].Commits.GetAll(startNode, force);
        }
    }
}

