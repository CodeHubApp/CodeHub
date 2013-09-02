using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Filters.Models;

namespace CodeHub.Controllers
{
    public class PullRequestsController : ListController<PullRequestModel, PullRequestsFilterModel>
    {
        public string User { get; private set; }

        public string Repo { get; private set; }

        public PullRequestsController(IListView<PullRequestModel> view, string user, string repo) 
            : base(view)
        {
            User = user;
            Repo = repo;

            Filter = Application.Account.GetFilter<PullRequestsFilterModel>(this);
        }

        public override void Update(bool force)
        {
            var state = Filter.IsOpen ? "open" : "closed";
            var data = Application.Client.Users[User].Repositories[Repo].PullRequests.GetAll(force, state: state);
            Model = new ListModel<PullRequestModel> {Data = data.Data, More = this.CreateMore(data)};
        }

        protected override void SaveFilterAsDefault(PullRequestsFilterModel filter)
        {
            Application.Account.AddFilter(this, filter);
        }
    }
}
