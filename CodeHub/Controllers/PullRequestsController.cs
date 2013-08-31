using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class PullRequestsController : ListController<PullRequestModel>
    {
        public string User { get; private set; }

        public string Repo { get; private set; }

        public PullRequestsController(IListView<PullRequestModel> view, string user, string repo) 
            : base(view)
        {
            User = user;
            Repo = repo;
        }

        public override void Update(bool force)
        {
            var data = Application.Client.Users[User].Repositories[Repo].PullRequests.GetAll(force);
            Model = new ListModel<PullRequestModel> {Data = data.Data, More = this.CreateMore(data)};
        }
    }
}
